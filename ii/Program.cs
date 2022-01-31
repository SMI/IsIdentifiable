using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using IsIdentifiable.Options;
using IsIdentifiable.Runners;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IsIdentifiable
{
    public static class Program
    {
        public static int Main(string[] args)
        {
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
                IsIdentifiableFileOptions>(args)
                .MapResult(
                          (IsIdentifiableRelationalDatabaseOptions o) => Run(o),
                          (IsIdentifiableDicomFileOptions o) => Run(o),
                          (IsIdentifiableFileOptions o) => Run(o),
                errors => 1);
            
            return res;
        }

       private static int Run(IsIdentifiableDicomFileOptions opts)
        {
            using (var runner = new DicomFileRunner(opts))
                return runner.Run();
        }

        private static int Run(IsIdentifiableRelationalDatabaseOptions opts)
        {
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();

            using (var runner = new DatabaseRunner(opts))
                return runner.Run();
        }
        private static int Run(IsIdentifiableFileOptions opts)
        {
            using (var runner = new FileRunner(opts))
                return runner.Run();
        }
    }
}
