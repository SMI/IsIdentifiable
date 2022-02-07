using System;
using System.Collections.Generic;
using System.Linq;
using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting;
using NLog;
using CsvHelper;
using System.IO;
using System.Globalization;

namespace IsIdentifiable.Runners
{
    /// <summary>
    /// Runner for reading data from CSV files and evaluating it for identifiable content
    /// </summary>
    public class FileRunner : IsIdentifiableAbstractRunner
    {
        private readonly IsIdentifiableFileOptions _opts;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a new instance for reading the CSV <see cref="IsIdentifiableFileOptions.File"/>
        /// and detecting identifiable data
        /// </summary>
        /// <param name="opts"></param>
        public FileRunner(IsIdentifiableFileOptions opts) : base(opts)
        {
            _opts = opts;
        }

        /// <summary>
        /// Opens <see cref="IsIdentifiableFileOptions.File"/> and evaluates all data within
        /// for identifiable information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override int Run()
        {
            using( var fs = new StreamReader(_opts.File.OpenRead()))
            {
                var culture = string.IsNullOrWhiteSpace(_opts.Culture) ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(_opts.Culture);
                
                using(var r = new CsvReader(fs,culture))
                {
                    if(!r.Read() || !r.ReadHeader())
                        throw new Exception("Csv file had no headers");

                    _logger.Info("Headers are:" + string.Join(",",r.HeaderRecord));

                    while(r.Read())
                    {
                        foreach (Failure failure in GetFailuresIfAny(r))
                            AddToReports(failure);
                    }
                
                    CloseReports();
                }

                return 0;
            }

        }

        private IEnumerable<Failure> GetFailuresIfAny(CsvReader r)
        {
            foreach(var h in r.HeaderRecord)
            {
                var parts = new List<FailurePart>();

                parts.AddRange(Validate(h, r[h]));

                if(parts.Any())
                    yield return new Failure(parts){
                        Resource = _opts.File.FullName,
                        ResourcePrimaryKey = "Unknown",
                        ProblemValue = r[h],
                        ProblemField = h };
            }

            DoneRows(1);
        }
    }
}
