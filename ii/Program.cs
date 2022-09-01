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
using NLog;
using Microsoft.Extensions.FileSystemGlobbing;
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
                IsIdentifiableFileGlobOptions,
                IsIdentifiableReviewerOptions>(args)
            .MapResult(
                (IsIdentifiableRelationalDatabaseOptions o) => Run(o),
                (IsIdentifiableDicomFileOptions o) => Run(o),
                (IsIdentifiableMongoOptions o) => Run(o),
                (IsIdentifiableFileGlobOptions o) => Run(o),
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
        var result = Inherit(opts);

        if (result != 0)
            return result;

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

        var result = Inherit(opts);

        if (result != 0)
            return result;

        using var runner = new DatabaseRunner(opts)
        {
            LogProgressEvery = 1000
        };
        return runner.Run();
    }

    private static int Run(IsIdentifiableFileGlobOptions opts)
    {
        var result = Inherit(opts);

        if (result != 0)
            return result;

        if (opts.File == null)
        {
            throw new Exception("You must specify a File or Directory indicate which files to work on");
        }           

        // if user has specified the full path of a file to -f
        if (File.Exists(opts.File))
        {
            // Run on the file
            ((IsIdentifiableFileOptions)opts).File = new FileInfo(opts.File);

            using var runner = new FileRunner(opts)
            {
                LogProgressEvery = 1000
            };

            return runner.Run();
        }

        // user has specified a directory as -f
        if(Directory.Exists(opts.File))
        {
            Matcher matcher = new();
            matcher.AddInclude(opts.Glob);
            result = 0;

            foreach (var match in matcher.GetResultsInFullPath(opts.File))
            {
                // set file to operate on to the current file
                ((IsIdentifiableFileOptions)opts).File = new FileInfo(match);

                using var runner = new FileRunner(opts)
                {
                    LogProgressEvery = 1000
                };
                var thisResult = runner.Run();

                // if we don't yet have any errors
                if (result == 0)
                {
                    // take any errors (or 0 if it went fine)
                    result = thisResult;
                }
            }

            return result;
        }
        else
        {
            throw new DirectoryNotFoundException($"Could not find a file or directory called '{opts.File}'");
        }
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
    private static int Inherit(IsIdentifiableBaseOptions opts)
    {
        if (GlobalOptions?.IsIdentifiableOptions != null)
        {
            opts.InheritValuesFrom(GlobalOptions.IsIdentifiableOptions);
        }

        return opts.UpdateConnectionStringsToUseTargets(out _);
    }

}