using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting.Reports;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace IsIdentifiable.Tests;

class StoreReportTests
{
    private MockFileSystem _fileSystem;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }

    [Test]
    public void TestReconstructionFromCsv()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions();
        var dir = _fileSystem.DirectoryInfo.New(".");

        opts.DestinationCsvFolder = dir.FullName;
        opts.TableName = "HappyOzz";
        opts.StoreReport = true;

        var report = new FailureStoreReport("HappyOzz", 1000, _fileSystem);
        report.AddDestinations(opts);

        var failure = new Failure(
            new FailurePart[]
            {
                new("Kansas", FailureClassification.Location, 12),
                new("Toto", FailureClassification.Location, 28)
            })
        {
            ProblemValue = "We aren't in Kansas anymore Toto",
            ProblemField = "Narrative",
            ResourcePrimaryKey = "1.2.3",
            Resource = "FunBooks.HappyOzz"
        };

        report.Add(failure);

        report.CloseReport();

        var created = dir.GetFiles("*HappyOzz*.csv").SingleOrDefault();

        Assert.That(created, Is.Not.Null);

        var failures2 = FailureStoreReport.Deserialize(created).ToArray();

        Assert.Multiple(() =>
        {
            //read failure ok
            Assert.That(failures2, Has.Length.EqualTo(1));

            Assert.That(failures2[0].ProblemValue, Is.EqualTo(failure.ProblemValue));
            Assert.That(failures2[0].ProblemField, Is.EqualTo(failure.ProblemField));
            Assert.That(failures2[0].ResourcePrimaryKey, Is.EqualTo(failure.ResourcePrimaryKey));
            Assert.That(failures2[0].Resource, Is.EqualTo(failure.Resource));

            //read parts ok
            Assert.That(failures2[0].Parts, Has.Count.EqualTo(2));

            Assert.That(failures2[0].Parts[0].Classification, Is.EqualTo(failure.Parts[0].Classification));
            Assert.That(failures2[0].Parts[0].Offset, Is.EqualTo(failure.Parts[0].Offset));
            Assert.That(failures2[0].Parts[0].Word, Is.EqualTo(failure.Parts[0].Word));

            Assert.That(failures2[0].Parts[1].Classification, Is.EqualTo(failure.Parts[1].Classification));
            Assert.That(failures2[0].Parts[1].Offset, Is.EqualTo(failure.Parts[1].Offset));
            Assert.That(failures2[0].Parts[1].Word, Is.EqualTo(failure.Parts[1].Word));
        });

    }


    [Test]
    public void Test_Includes()
    {
        var origin = "this word fff is the problem";

        var part = new FailurePart("fff", FailureClassification.Organization, origin.IndexOf("fff"));

        Assert.Multiple(() =>
        {
            Assert.That(part.Includes(0), Is.False);
            Assert.That(part.Includes(9), Is.False);
            Assert.That(part.Includes(10), Is.True);
            Assert.That(part.Includes(11), Is.True);
            Assert.That(part.Includes(12), Is.True);
            Assert.That(part.Includes(13), Is.False);
        });
    }

    [Test]
    public void Test_IncludesSingleChar()
    {
        var origin = "this word f is the problem";

        var part = new FailurePart("f", FailureClassification.Organization, origin.IndexOf("f"));

        Assert.Multiple(() =>
        {
            Assert.That(part.Includes(0), Is.False);
            Assert.That(part.Includes(9), Is.False);
            Assert.That(part.Includes(10), Is.True);
            Assert.That(part.Includes(11), Is.False);
            Assert.That(part.Includes(12), Is.False);
            Assert.That(part.Includes(13), Is.False);
        });
    }



    [Test]
    public void Test_HaveSameProblem()
    {
        var f1 = new Failure(new List<FailurePart>())
        {
            ProblemValue = "Happy fun times",
            ProblemField = "Jokes",
            Resource = "MyTable",
            ResourcePrimaryKey = "1.2.3"
        };
        var f2 = new Failure(new List<FailurePart>())
        {
            ProblemValue = "Happy fun times",
            ProblemField = "Jokes",
            Resource = "MyTable",
            ResourcePrimaryKey = "9.9.9" //same problem different record (are considered to have the same problem)
        };
        var f3 = new Failure(new List<FailurePart>())
        {
            ProblemValue = "Happy times", //different because input value is different
            ProblemField = "Jokes",
            Resource = "MyTable",
            ResourcePrimaryKey = "1.2.3"
        };
        var f4 = new Failure(new List<FailurePart>())
        {
            ProblemValue = "Happy fun times",
            ProblemField = "SensitiveJokes", //different because other column
            Resource = "MyTable",
            ResourcePrimaryKey = "1.2.3"
        };

        Assert.Multiple(() =>
        {
            Assert.That(f1.HaveSameProblem(f2), Is.True);
            Assert.That(f1.HaveSameProblem(f3), Is.False);
            Assert.That(f1.HaveSameProblem(f4), Is.False);
        });
    }

}
