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
        private bool firstTime = true;

        /// <summary>
        /// Reads all values in <paramref name="filePath"/>.  The contents of each line
        /// will be used as an 'ignore' value by IsIdentifiable (i.e. a list of false
        /// positives or values that match rules but should be ignored anyway).
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Returns all 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllowlist()
        {
            if (!firstTime)
                throw new Exception("Allow list has already been read from file.  This method should only be called once");

            while (_reader.Read())
                yield return _reader[0];

            firstTime = false;
        }

        /// <summary>
        /// Closes the file and disposes of IO handles and streams
        /// </summary>
        public void Dispose()
        {
            _reader.Dispose();
            _streamreader.Dispose();
        }
    }
}