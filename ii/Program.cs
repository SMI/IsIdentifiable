using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using FellowOakDicom;
using IsIdentifiable.Options;
using IsIdentifiable.Redacting;
using IsIdentifiable.Runners;
using System.Linq;
using YamlDotNet.Serialization;

namespace IsIdentifiable;

public static class Program
{
    const string SettingsFile = "ii-settings.yaml";
    static GlobalOptions? GlobalOptions;


    public static string? CutSettingsFileArgs(string []args, out string[] newArgs )
    {
        var idx = Array.IndexOf(args, "-y");

        // do we have 2 elements "-y" then "somepath"?
        if (idx != -1 && idx < args.Length - 1)
        {
            var settingsFileLocation = args[idx + 1];
                        
            // remove -y myfile
            newArgs = args.Take(idx).ToArray();

            foreach (var after in args.Skip(idx + 2))
            {
                newArgs = newArgs.Append(after).ToArray();
            }

            return settingsFileLocation;
        }

        // no change
        newArgs = args;
        return null;
    }

    public static int Main(string[] args)
    {

        string? explicitLocation = CutSettingsFileArgs(args, out var newArgs);
        args = newArgs;

        var settingsFileLocation = SettingsFile;

        // if user specified -y on command line
        if (!string.IsNullOrWhiteSpace(explicitLocation))
        {
            //verify the file they told us about is real
            if (!File.Exists(explicitLocation))
            {
                Console.Error.WriteLine($"Could not find file: {explicitLocation}");
                return 1;
            }

            // if it is then use it instead of ii-settings.yaml
            settingsFileLocation = explicitLocation;
        }

        // load GlobalOptions
        if(File.Exists(settingsFileLocation))
        {
            try
            {
                GlobalOptions = Deserialize(settingsFileLocation);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not deserialize {SettingsFile}:{ex}");
                return 1;
            }
        }
        
        // Disable fo-dicom's DICOM validation globally from here
        new DicomSetupBuilder().SkipValidation();

        ParserSettings defaults = Parser.Default.Settings;
        using var parser = new Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.CaseSensitive = false;
            settings.EnableDashDash = defaults.EnableDashDash;
            settings.HelpWriter = defaults.HelpWriter;
            settings.IgnoreUnknownArguments = false;
            settings.MaximumDisplayWidth = defaults.MaximumDisplayWidth;
            settings.ParsingCulture = defaults.ParsingCulture;
        });

        var res = parser.ParseArguments<IsIdentifiableRelationalDatabaseOptions,
                IsIdentifiableDicomFileOptions,
                IsIdentifiableMongoOptions,
                IsIdentifiableFileOptions,
                IsIdentifiableReviewerOptions>(args)
            .MapResult(
                (IsIdentifiableRelationalDatabaseOptions o) => Run(o),
                (IsIdentifiableDicomFileOptions o) => Run(o),
                (IsIdentifiableMongoOptions o) => Run(o),
                (IsIdentifiableFileOptions o) => Run(o),
                (IsIdentifiableReviewerOptions o) => Run(o),
                
                // return exit code 0 for user requests for help
                errors => args.Any(a=>a.Equals("--help",StringComparison.InvariantCultureIgnoreCase)) ? 0: 1);
            
        return res;
    }

    public static GlobalOptions? Deserialize(string settingsFileLocation)
    {
        IDeserializer deserializer = new DeserializerBuilder()
                        .IgnoreUnmatchedProperties()
                        .Build();

        return deserializer.Deserialize<GlobalOptions>(File.ReadAllText(settingsFileLocation));
    }

    private static int Run(IsIdentifiableReviewerOptions opts)
    {
        Inherit(opts);

        var reviewer = new ReviewerRunner(GlobalOptions?.IsIdentifiableOptions, opts);
        return reviewer.Run();
    }


    private static int Run(IsIdentifiableDicomFileOptions opts)
    {
        Inherit(opts);

        using var runner = new DicomFileRunner(opts)
        {
            ThrowOnError = false,
            LogProgressEvery = 1000
        };
        return runner.Run();
    }

    private static int Run(IsIdentifiableRelationalDatabaseOptions opts)
    {
        ImplementationManager.Load<MicrosoftSQLImplementation>();
        ImplementationManager.Load<MySqlImplementation>();
        ImplementationManager.Load<PostgreSqlImplementation>();
        ImplementationManager.Load<OracleImplementation>();

        Inherit(opts);

        using var runner = new DatabaseRunner(opts)
        {
            LogProgressEvery = 1000
        };
        return runner.Run();
    }
    private static int Run(IsIdentifiableFileOptions opts)
    {
        Inherit(opts);

        using var runner = new FileRunner(opts)
        {
            LogProgressEvery = 1000
        };
        return runner.Run();
    }
    private static int Run(IsIdentifiableMongoOptions opts)
    {
        Inherit(opts);

        using var runner = new MongoRunner(opts)
        {
            LogProgressEvery = 1000
        };
        return runner.Run();
    }
    private static void Inherit(IsIdentifiableReviewerOptions opts)
    {
        if(GlobalOptions?.IsIdentifiableReviewerOptions != null)
        {
            opts.InheritValuesFrom(GlobalOptions.IsIdentifiableReviewerOptions);
        }
    }
    private static void Inherit(IsIdentifiableBaseOptions opts)
    {
        if (GlobalOptions?.IsIdentifiableOptions != null)
        {
            opts.InheritValuesFrom(GlobalOptions.IsIdentifiableOptions);
        }
    }

}