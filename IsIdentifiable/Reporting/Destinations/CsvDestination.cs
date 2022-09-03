using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using IsIdentifiable.Options;

namespace IsIdentifiable.Reporting.Destinations;

/// <summary>
/// <see cref="ReportDestination"/> that outputs IsIdentifiable reports to a text file
/// </summary>
public class CsvDestination : ReportDestination
{
    /// <summary>
    /// The location of the CSV output file created
    /// </summary>
    public string ReportPath { get; private set; }

    private readonly IFileSystem _fileSystem;
    private StreamWriter _streamwriter;
    private CsvWriter _csvWriter;
    private readonly CsvConfiguration _csvConfiguration;

    private readonly object _oHeaderLock = new object();
    private bool _headerWritten;

    /// <summary>
    /// Creates a new report destination in which values/aggregates are written to CSV (at <see cref="ReportPath"/>)
    /// </summary>
    /// <param name="options"></param>
    /// <param name="reportName"></param>
    /// <param name="csvConfiguration"></param>
    /// <param name="addTimestampToFilename">True to add the time to the CSV filename generated</param>
    /// <param name="fileSystem"></param>
    public CsvDestination(
        IsIdentifiableBaseOptions options,
        string reportName,
        CsvConfiguration csvConfiguration = null,
        bool addTimestampToFilename = true,
        IFileSystem fileSystem = null
    )
        : base(options)
    {
        _fileSystem = fileSystem ?? new FileSystem();

        var destDir = _fileSystem.DirectoryInfo.FromDirectoryName(Options.DestinationCsvFolder);

        if (!destDir.Exists)
            destDir.Create();

        ReportPath = addTimestampToFilename ?
            _fileSystem.Path.Combine(destDir.FullName, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm}-{reportName}.csv") :
            _fileSystem.Path.Combine(destDir.FullName, $"{reportName}.csv");

        _csvConfiguration = csvConfiguration;
    }

    /// <summary>
    /// Creates new report destination in which values/aggregates are written to CSV <paramref name="file"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="file"></param>
    /// <param name="csvConfiguration"></param>
    /// <param name="fileSystem"></param>
    public CsvDestination(
        IsIdentifiableBaseOptions options,
        IFileInfo file,
        CsvConfiguration csvConfiguration = null,
        IFileSystem fileSystem = null
    )
        : base(options)
    {
        ReportPath = file.FullName;
        _csvConfiguration = csvConfiguration;
        _fileSystem = fileSystem ?? new FileSystem();
    }

    /// <summary>
    /// Writes the headings required by the report into the CSV at <see cref="ReportPath"/>.
    /// If the file does not exist yet it will be automatically created
    /// </summary>
    /// <param name="headers"></param>
    public override void WriteHeader(params string[] headers)
    {
        lock (_oHeaderLock)
        {
            if (_headerWritten)
                return;

            _headerWritten = true;

            var csvFile = _fileSystem.FileInfo.FromFileName(ReportPath);
            CsvConfiguration csvconf;
            string sep = Options.DestinationCsvSeparator;

            if (_csvConfiguration != null)
            {
                csvconf = _csvConfiguration;
            }
            // If there is an overriding separator and it's not a comma, then use the users desired delimiter string
            else if (!string.IsNullOrWhiteSpace(sep) && !sep.Trim().Equals(","))
            {
                csvconf = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
                {
                    Delimiter =
                        sep.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n"),
                    ShouldQuote = _ => false,
                };
            }
            else
            {
                csvconf = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture);
            }

            _streamwriter = new StreamWriter(csvFile.OpenWrite());
            _csvWriter = new CsvWriter(_streamwriter, csvconf);
            WriteRow(headers);
        }
    }

    /// <summary>
    /// Appends the report aggregates/items data to the CSV
    /// </summary>
    /// <param name="items"></param>
    public override void WriteItems(DataTable items)
    {
        if (!_headerWritten)
            WriteHeader((from dc in items.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray());

        foreach (DataRow row in items.Rows)
            WriteRow(row.ItemArray);
    }

    /// <summary>
    /// Flushes and disposes of IO handles to <see cref="ReportPath"/>
    /// </summary>
    public override void Dispose()
    {
        _csvWriter?.Dispose();
        _streamwriter?.Dispose();
    }

    private void WriteRow(IEnumerable<object> rowItems)
    {
        foreach (var item in rowItems)
            _csvWriter.WriteField(StripWhitespace(item));

        _csvWriter.NextRecord();
    }
}