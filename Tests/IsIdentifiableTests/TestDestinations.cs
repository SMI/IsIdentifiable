﻿using IsIdentifiable.Options;
using IsIdentifiable.Reporting;
using IsIdentifiable.Reporting.Destinations;
using IsIdentifiable.Reporting.Reports;
using NUnit.Framework;
using System;
using System.Data;
using System.IO.Abstractions.TestingHelpers;

namespace IsIdentifiableTests;

internal class TestDestinations
{
    private MockFileSystem _fileSystem;
    private const string OUT_DIR = "test";

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }

    [Test]
    public void TestCsvDestination_Normal()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions { DestinationCsvFolder = OUT_DIR };
        var dest = new CsvDestination(opts, "test", _fileSystem, false);

        var report = new TestFailureReport(dest);
        report.WriteToDestinations();
        report.CloseReport();

        string fileCreatedContents = _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(OUT_DIR, "test.csv"));
        fileCreatedContents = fileCreatedContents.Replace("\r\n", Environment.NewLine);

        TestHelpers.AreEqualIgnoringLineEndings(@"col1,col2
""cell1 with some new 
 lines and 	 tabs"",cell2
", fileCreatedContents);
    }

    [Test]
    public void TestCsvDestination_NormalButNoWhitespace()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions { DestinationNoWhitespace = true, DestinationCsvFolder = OUT_DIR };
        var dest = new CsvDestination(opts, "test", _fileSystem, false);

        var report = new TestFailureReport(dest);
        report.WriteToDestinations();
        report.CloseReport();

        var fileCreatedContents = _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(OUT_DIR, "test.csv"));
        fileCreatedContents = fileCreatedContents.Replace("\r\n", Environment.NewLine);

        TestHelpers.AreEqualIgnoringLineEndings(@"col1,col2
cell1 with some new lines and tabs,cell2
", fileCreatedContents);
    }

    [Test]
    public void TestCsvDestination_Tabs()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions
        {
            // This is slash t, not an tab
            DestinationCsvSeparator = "\\t",
            DestinationNoWhitespace = true,
            DestinationCsvFolder = OUT_DIR,
        };

        var dest = new CsvDestination(opts, "test", _fileSystem, false);

        var report = new TestFailureReport(dest);
        report.WriteToDestinations();
        report.CloseReport();

        string fileCreatedContents = _fileSystem.File.ReadAllText(_fileSystem.Path.Combine(OUT_DIR, "test.csv"));
        fileCreatedContents = fileCreatedContents.Replace("\r\n", Environment.NewLine);

        TestHelpers.AreEqualIgnoringLineEndings(@"col1	col2
cell1 with some new lines and tabs	cell2
", fileCreatedContents);
    }

    [Test]
    public void TestCsvDestination_DisposeWithoutUsing()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions { DestinationCsvFolder = OUT_DIR };
        var dest = new CsvDestination(opts, "test", _fileSystem, false);
        dest.Dispose();
    }
}

internal class TestFailureReport : IFailureReport
{
    private readonly IReportDestination _dest;

    private readonly DataTable _dt = new DataTable();

    public TestFailureReport(IReportDestination dest)
    {
        _dest = dest;

        _dt.Columns.Add("col1");
        _dt.Columns.Add("col2");
        _dt.Rows.Add("cell1 with some new \r\n lines and \t tabs", "cell2");
    }


    public void AddDestinations(IsIdentifiableBaseOptions options) { }

    public void DoneRows(int numberDone) { }

    public void Add(Failure failure) { }

    public void CloseReport()
    {
        _dest.Dispose();
    }

    public void WriteToDestinations()
    {
        _dest.WriteItems(_dt);
    }
}