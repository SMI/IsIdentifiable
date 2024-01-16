using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using IsIdentifiable.Options;
using IsIdentifiable.Redacting;
using NUnit.Framework;
using System;
using System.IO.Abstractions.TestingHelpers;

namespace IsIdentifiable.Tests.ReviewerTests;

public class UnattendedTests
{
    private MockFileSystem _fileSystem;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ImplementationManager.Load<MicrosoftSQLImplementation>();
        ImplementationManager.Load<MySqlImplementation>();
        ImplementationManager.Load<PostgreSqlImplementation>();
        ImplementationManager.Load<OracleImplementation>();
    }

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }

    [Test]
    public void NoFileToProcess_Throws()
    {
        var ex = Assert.Throws<Exception>(() => new UnattendedReviewer(new IsIdentifiableReviewerOptions(), null, new IgnoreRuleGenerator(_fileSystem), new RowUpdater(_fileSystem), _fileSystem));
        Assert.That(ex.Message, Is.EqualTo("Unattended requires a file of errors to process"));
    }

    [Test]
    public void NonExistantFileToProcess_Throws()
    {
        var ex = Assert.Throws<System.IO.FileNotFoundException>(() => new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = "troll.csv"
        }, null, new IgnoreRuleGenerator(_fileSystem), new RowUpdater(_fileSystem), _fileSystem));
        Assert.That(ex.Message, Does.Contain("Could not find Failures file"));
    }

    [Test]
    public void NoTarget_Throws()
    {
        var fi = "myfile.txt";
        _fileSystem.File.WriteAllText(fi, "fff");

        var ex = Assert.Throws<Exception>(() => new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = fi
        }, null, new IgnoreRuleGenerator(_fileSystem), new RowUpdater(_fileSystem), _fileSystem));
        Assert.That(ex.Message, Does.Contain("A single Target must be supplied for database updates"));
    }

    [Test]
    public void NoOutputPath_Throws()
    {
        var fi = "myfile.txt";
        _fileSystem.File.WriteAllText(fi, "fff");

        var ex = Assert.Throws<Exception>(() => new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = fi
        }, new Target(), new IgnoreRuleGenerator(_fileSystem), new RowUpdater(_fileSystem), _fileSystem));
        Assert.That(ex.Message, Does.Contain("An output path must be specified "));
    }


    [Test]
    public void Passes_NoFailures()
    {
        var fi = "myfile.csv";
        _fileSystem.File.WriteAllText(fi, "fff");

        var fiOut = "out.csv";

        var reviewer = new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = fi,
            UnattendedOutputPath = fiOut
        }, new Target(), new IgnoreRuleGenerator(_fileSystem), new RowUpdater(_fileSystem), _fileSystem);

        Assert.Multiple(() =>
        {
            Assert.That(reviewer.Run(), Is.EqualTo(0));

            //just the headers
            Assert.That(_fileSystem.File.ReadAllText(fiOut).TrimEnd(), Is.EqualTo("Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets").IgnoreCase);
        });
    }

    [Test]
    public void Passes_FailuresAllUnprocessed()
    {
        var inputFile = @"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets
FunBooks.HappyOzz,1.2.3,Narrative,We aren't in Kansas anymore Toto,Kansas###Toto,Location###Location,13###28";

        var fi = "myfile.csv";
        _fileSystem.AddFile(fi, new MockFileData(inputFile));

        var fiOut = "out.csv";

        var reviewer = new UnattendedReviewer(
            new IsIdentifiableReviewerOptions()
            {
                FailuresCsv = fi,
                UnattendedOutputPath = fiOut,
            },
            new Target(),
            new IgnoreRuleGenerator(fileSystem: _fileSystem),
            new RowUpdater(fileSystem: _fileSystem),
            _fileSystem
        );

        Assert.That(reviewer.Run(), Is.EqualTo(0));

        //all that we put in is unprocessed so should come out the same
        TestHelpers.AreEqualIgnoringCaseAndLineEndings(inputFile, _fileSystem.File.ReadAllText(fiOut).TrimEnd());

        Assert.Multiple(() =>
        {
            Assert.That(reviewer.Total, Is.EqualTo(1));
            Assert.That(reviewer.Ignores, Is.EqualTo(0));
            Assert.That(reviewer.Unresolved, Is.EqualTo(1));
            Assert.That(reviewer.Updates, Is.EqualTo(0));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Passes_FailuresAllIgnored(bool rulesOnly)
    {
        var inputFile = @"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets
FunBooks.HappyOzz,1.2.3,Narrative,We aren't in Kansas anymore Toto,Kansas###Toto,Location###Location,13###28";

        var fi = "myfile.csv";
        _fileSystem.File.WriteAllText(fi, inputFile);

        var fiOut = "out.csv";

        var fiAllowlist = IgnoreRuleGenerator.DefaultFileName;

        //add a Allowlist to ignore these
        _fileSystem.File.WriteAllText(fiAllowlist,
            @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^We\ aren't\ in\ Kansas\ anymore\ Toto$");

        var reviewer = new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = fi,
            UnattendedOutputPath = fiOut,
            OnlyRules = rulesOnly
        }, new Target(), new IgnoreRuleGenerator(fileSystem: _fileSystem), new RowUpdater(fileSystem: _fileSystem), _fileSystem);

        Assert.Multiple(() =>
        {
            Assert.That(reviewer.Run(), Is.EqualTo(0));

            //headers only since Allowlist eats the rest
            Assert.That(_fileSystem.File.ReadAllText(fiOut).TrimEnd(), Is.EqualTo(@"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets").IgnoreCase);

            Assert.That(reviewer.Total, Is.EqualTo(1));
            Assert.That(reviewer.Ignores, Is.EqualTo(1));
            Assert.That(reviewer.Unresolved, Is.EqualTo(0));
            Assert.That(reviewer.Updates, Is.EqualTo(0));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Passes_FailuresAllUpdated(bool ruleCoversThis)
    {
        var inputFile = @"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets
FunBooks.HappyOzz,1.2.3,Narrative,We aren't in Kansas anymore Toto,Kansas###Toto,Location###Location,13###28";

        var fi = "myfile.csv";
        _fileSystem.File.WriteAllText(fi, inputFile);

        var fiOut = "out.csv";

        var fiReportlist = RowUpdater.DefaultFileName;

        //add a Reportlist to UPDATE these
        if (ruleCoversThis)
        {
            _fileSystem.File.WriteAllText(fiReportlist,
                @"
- Action: Ignore
  IfColumn: Narrative
  IfPattern: ^We\ aren't\ in\ Kansas\ anymore\ Toto$");
        }

        var reviewer = new UnattendedReviewer(new IsIdentifiableReviewerOptions()
        {
            FailuresCsv = fi,
            UnattendedOutputPath = fiOut,
            OnlyRules = true //prevents it going to the database
        }, new Target(), new IgnoreRuleGenerator(fileSystem: _fileSystem), new RowUpdater(fileSystem: _fileSystem), _fileSystem);

        Assert.That(reviewer.Run(), Is.EqualTo(0));

        //it matches the UPDATE rule but since OnlyRules is true it didn't actually update the database! so the record should definitely be in the output

        if (!ruleCoversThis)
        {
            // no rule covers this so the miss should appear in the output file

            TestHelpers.AreEqualIgnoringCaseAndLineEndings(@"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets
FunBooks.HappyOzz,1.2.3,Narrative,We aren't in Kansas anymore Toto,Kansas###Toto,Location###Location,13###28", _fileSystem.File.ReadAllText(fiOut).TrimEnd());
        }
        else
        {

            // a rule covers this so even though we do not update the database there shouldn't be a 'miss' in the output file

            TestHelpers.AreEqualIgnoringCaseAndLineEndings(@"Resource,ResourcePrimaryKey,ProblemField,ProblemValue,PartWords,PartClassifications,PartOffsets",
                _fileSystem.File.ReadAllText(fiOut).TrimEnd());

        }

        Assert.Multiple(() =>
        {
            Assert.That(reviewer.Total, Is.EqualTo(1));
            Assert.That(reviewer.Ignores, Is.EqualTo(0));
            Assert.That(reviewer.Unresolved, Is.EqualTo(ruleCoversThis ? 0 : 1));
            Assert.That(reviewer.Updates, Is.EqualTo(ruleCoversThis ? 1 : 0));
        });
    }
}
