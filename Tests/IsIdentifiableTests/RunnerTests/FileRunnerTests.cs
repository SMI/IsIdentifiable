using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting.Reports;
using IsIdentifiable.Runners;
using Moq;
using NUnit.Framework;
using System.IO.Abstractions.TestingHelpers;

namespace IsIdentifiable.Tests.RunnerTests;

class FileRunnerTests
{
    private MockFileSystem _fileSystem;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }


    [Test]
    public void FileRunner_CsvWithCHI()
    {
        var fi = _fileSystem.FileInfo.New("testfile.csv");
        using (var s = fi.CreateText())
        {
            s.WriteLine("Fish,Chi,Bob");
            s.WriteLine("123,0102821172,32 Ankleberry lane");
            s.Flush();
            s.Close();
        }

        var runner = new FileRunner(new IsIdentifiableFileOptions() { FilePath = fi.FullName, StoreReport = true }, _fileSystem);

        var reporter = new Mock<IFailureReport>(MockBehavior.Strict);

        reporter.Setup(f => f.Add(It.IsAny<Failure>())).Callback<Failure>(f => Assert.That(f.ProblemValue, Is.EqualTo("0102821172")));
        reporter.Setup(f => f.DoneRows(1));
        reporter.Setup(f => f.CloseReport());


        runner.Reports.Add(reporter.Object);

        runner.Run();

        reporter.Verify();
    }

    [Test]
    public void FileRunner_TopX()
    {
        var fi = _fileSystem.FileInfo.New("testfile.csv");
        using (var s = fi.CreateText())
        {
            s.WriteLine("Fish,Chi,Bob");

            // create a 100 line file
            for (var i = 0; i < 100; i++)
                s.WriteLine("123,0102821172,32 Ankleberry lane");

            s.Flush();
            s.Close();
        }

        var runner = new FileRunner(new IsIdentifiableFileOptions() { FilePath = fi.FullName, StoreReport = true, Top = 22 }, _fileSystem);

        var reporter = new Mock<IFailureReport>(MockBehavior.Strict);

        var done = 0;

        reporter.Setup(f => f.Add(It.IsAny<Failure>())).Callback<Failure>(f => Assert.That(f.ProblemValue, Is.EqualTo("0102821172")));
        reporter.Setup(f => f.DoneRows(1)).Callback(() => done++);
        reporter.Setup(f => f.CloseReport());


        runner.Reports.Add(reporter.Object);

        runner.Run();

        reporter.Verify();

        Assert.That(done, Is.EqualTo(22));
    }
}
