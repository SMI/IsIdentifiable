using System;
using System.Collections.Generic;
using System.Data;
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

    private System.IO.StreamWriter _streamwriter;
    private readonly CsvConfiguration _csvConfiguration;
    private CsvWriter _csvWriter;

    private readonly object _oHeaderLock = new();
    private bool _headerWritten;

    /// <summary>
    /// Creates a new report destination in which values/aggregates are written to CSV (at <see cref="ReportPath"/>)
    /// </summary>
    /// <param name="options"></param>
    /// <param name="reportName"></param>
    /// <param name="addTimestampToFilename">True to add the time to the CSV filename generated</param>
    /// <param name="csvConfiguration"></param>
    /// <param name="fileSystem"></param>
    public CsvDestination(
        IsIdentifiableBaseOptions options,
        string reportName,
        IFileSystem fileSystem,
        bool addTimestampToFilename = true,
        CsvConfiguration csvConfiguration = null
    )
        : base(options, fileSystem)
    {
        var destDir = FileSystem.DirectoryInfo.New(Options.DestinationCsvFolder);

        if (!destDir.Exists)
            destDir.Create();

        ReportPath = addTimestampToFilename ?
            FileSystem.Path.Combine(destDir.FullName, $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm}-{reportName}.csv") :
            FileSystem.Path.Combine(destDir.FullName, $"{reportName}.csv");

        _csvConfiguration = csvConfiguration;
    }

    /// <summary>
    /// Creates new report destination in which values/aggregates are written to CSV <paramref name="file"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="file"></param>
    /// <param name="fileSystem"></param>
    /// <param name="csvConfiguration"></param>
    public CsvDestination(
        IsIdentifiableBaseOptions options,
        IFileInfo file,
        IFileSystem fileSystem,
        CsvConfiguration csvConfiguration = null
    )
        : base(options, fileSystem)
    {
        ReportPath = file.FullName;
        _csvConfiguration = csvConfiguration; ;
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

            var csvFile = FileSystem.FileInfo.New(ReportPath);
            var sep = Options.DestinationCsvSeparator;

            CsvConfiguration csvconf;
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

            var stream = csvFile.OpenWrite();
            _streamwriter = new System.IO.StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamwriter,csvconf);
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