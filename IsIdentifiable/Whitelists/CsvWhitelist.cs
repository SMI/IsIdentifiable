using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace IsIdentifiable.Allowlists
{
    /// <summary>
    /// A Allowlist source which returns the values in the first column of the provided Csv file.  The file must be properly escaped
    /// if it has commas in fields etc.  There must be no header record.
    /// </summary>
    public class CsvAllowlist : IAllowlistSource
    {
        private readonly StreamReader _streamreader;
        private readonly CsvReader _reader;

        public CsvAllowlist(string filePath)
        {
            if(!File.Exists(filePath))
                throw new Exception("Could not find Allowlist file at '" + filePath +"'");

            _streamreader = new StreamReader(filePath);
            _reader = new CsvReader(_streamreader,new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
            {
                HasHeaderRecord=false
            });
        }

        public IEnumerable<string> GetAllowlist()
        {
            while (_reader.Read())
                yield return _reader[0];
        }

        public void Dispose()
        {
            _reader.Dispose();
            _streamreader.Dispose();
        }
    }
}