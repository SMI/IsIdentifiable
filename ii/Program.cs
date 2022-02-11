using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using IsIdentifiable.Options;
using IsIdentifiable.Runners;
using IsIdentifiableReviewer;
using YamlDotNet.Serialization;

namespace IsIdentifiable
{
    public static class Program
    {
        const string SettingsFile = "ii-settings.yaml";
        static GlobalOptions? GlobalOptions;

        public static int Main(string[] args)
        {
            
            if(File.Exists(SettingsFile))
            {
                try
                {
                    var d = new Deserializer();
                    GlobalOptions = d.Deserialize<GlobalOptions>(File.ReadAllText(SettingsFile));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not deserialize {SettingsFile}:{ex}");
                    return 1;
                }
            }

            ParserSettings defaults = Parser.Default.Settings;
            var parser = new Parser(settings =>
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
                IsIdentifiableMongoOptions,
                IsIdentifiableReviewerOptions>(args)
                .MapResult(
                          (IsIdentifiableRelationalDatabaseOptions o) => Run(o),
                          (IsIdentifiableDicomFileOptions o) => Run(o),
                          (IsIdentifiableFileOptions o) => Run(o),
                          (IsIdentifiableMongoOptions o) => Run(o),
                          (IsIdentifiableReviewerOptions o) => Run(o),
                errors => 1);
            
            return res;
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

            using (var runner = new DicomFileRunner(opts))
                return runner.Run();
        }

        private static int Run(IsIdentifiableRelationalDatabaseOptions opts)
        {
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();

            Inherit(opts);

            using (var runner = new DatabaseRunner(opts))
                return runner.Run();
        }
        private static int Run(IsIdentifiableFileOptions opts)
        {
            Inherit(opts);

            using (var runner = new FileRunner(opts))
                return runner.Run();
        }
        private static int Run(IsIdentifiableMongoOptions opts)
        {
            Inherit(opts);

            using (var runner = new MongoRunner(opts))
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
}
