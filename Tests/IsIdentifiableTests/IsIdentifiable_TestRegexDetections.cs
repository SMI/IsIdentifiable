using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Rules;
using IsIdentifiable.Runners;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;

namespace IsIdentifiable.Tests;

public class IsIdentifiableRunnerTests
{
    private MockFileSystem _fileSystem;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = new MockFileSystem();
    }

    [Test]
    public void TestChiInString()
    {
        var runner = new TestRunner("hey there,0101010101 excited to see you", _fileSystem);
        runner.Run();

        var p = runner.ResultsOfValidate.Single();

        Assert.Multiple(() =>
        {
            Assert.That(p.Word, Is.EqualTo("0101010101"));
            Assert.That(p.Offset, Is.EqualTo(10));
        });
    }

    [Test]
    public void TestChiBadDate()
    {
        var runner = new TestRunner("2902810123 would be a CHI if 1981 had been a leap year", _fileSystem);
        runner.Run();
        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    [Test]
    public void TestCaching()
    {
        var runner = new TestRunner("hey there,0101010101 excited to see you", _fileSystem);
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(0));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(1));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(1));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(1));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(2));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(1));
        });

        runner.ValueToTest = "ffffff";
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(2));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(2));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(3));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(2));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(4));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(2));
        });

        runner.FieldToTest = "OtherField";
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(4));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(3));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(5));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(3));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(6));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(3));
        });
    }
    [Test]
    public void Test_NoCaching()
    {
        var runner = new TestRunner("hey there,0101010101 excited to see you", _fileSystem)
        {
            MaxValidationCacheSize = 0
        };

        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(0));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(1));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(0));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(2));
        });
        runner.Run();
        Assert.Multiple(() =>
        {
            Assert.That(runner.ValidateCacheHits, Is.EqualTo(0));
            Assert.That(runner.ValidateCacheMisses, Is.EqualTo(3));
        });
        runner.Run();
    }
    [TestCase("DD3 7LB")]
    [TestCase("dd3 7lb")]
    [TestCase("dd37lb")]
    public void IsIdentifiable_TestPostcodes(string code)
    {
        var runner = new TestRunner($"Patient lives at {code}", _fileSystem);
        runner.Run();

        var p = runner.ResultsOfValidate.Single();

        Assert.Multiple(() =>
        {
            //this would be nice
            Assert.That(p.Word, Is.EqualTo(code));
            Assert.That(p.Offset, Is.EqualTo(17));
            Assert.That(p.Classification, Is.EqualTo(FailureClassification.Postcode));
        });
    }

    [TestCase("DD3 7LB")]
    [TestCase("dd3 7lb")]
    [TestCase("dd37lb")]
    public void IsIdentifiable_TestPostcodes_AllowlistDD3(string code)
    {

        var runner = new TestRunner($"Patient lives at {code}", _fileSystem);

        runner.LoadRules(
            @"
BasicRules:
  - Action: Ignore
    IfPattern: DD3");

        runner.Run();

        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    [TestCase("DD3 7LB")]
    [TestCase("dd3 7lb")]
    [TestCase("dd37lb")]
    public void IsIdentifiable_TestPostcodes_IgnorePostcodesFlagSet(string code)
    {
        //since allow postcodes flag is set
        var runner = new TestRunner($"Patient lives at {code}", new TestOpts() { IgnorePostcodes = true }, _fileSystem);
        runner.Run();

        //there won't be any failure results reported
        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }


    //no longer detected, that's fine
    //[TestCase("Patient_lives_at_DD28DD", "DD28DD")]
    [TestCase("^DD28DD^", "DD28DD")]
    [TestCase("dd3^7lb", "dd3 7lb")]
    public void IsIdentifiable_TestPostcodes_EmbeddedInText(string find, string expectedMatch)
    {
        var runner = new TestRunner(find, _fileSystem);
        runner.Run();

        var p = runner.ResultsOfValidate.Single();

        Assert.Multiple(() =>
        {
            //this would be nice
            Assert.That(p.Word, Is.EqualTo(expectedMatch));
            Assert.That(p.Classification, Is.EqualTo(FailureClassification.Postcode));
            Assert.That(runner.CountOfFailureParts, Is.EqualTo(1));
        });
    }

    [TestCase("dd3000")]
    [TestCase("dd3 000")]
    [TestCase("1444DD2011FD1118E63006097D2DF4834C9D2777977D811907000065B840D9CA50000000837000000FF0100A601000000003800A50900000700008001000000AC020000008000000D0000805363684772696400A8480000E6FBFFFF436174616C6F6775654974656D07000000003400A50900000700008002000000A402000000800000090000805363684772696400A84800001E2D0000436174616C6F67756500000000008000A50900000700008003000000520000000180000058000080436F6E74726F6C00A747000")]
    public void IsIdentifiable_TestNotAPostcode(string code)
    {

        var runner = new TestRunner($"Patient lives at {code}", _fileSystem);
        runner.Run();

        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }


    [TestCase("Friday, 29 May 2015", "29 May", "May 2015", null)]
    [TestCase("Friday, 29 May 2015 05:50", "29 May", "May 2015", null)]
    [TestCase("Friday, 29 May 2015 05:50 AM", "29 May", "May 2015", null)]
    [TestCase("Friday, 29th May 2015 5:50", "29th May", "May 2015", null)]
    [TestCase("Friday, May 29th 2015 5:50 AM", "May 29th", null, null)]
    [TestCase("Friday, 29-May-2015 05:50:06", "29-May", "May-2015", null)]
    [TestCase("05/29/2015 05:50", "05/29/2015", null, null)]
    [TestCase("05-29-2015 05:50 AM", "05-29-2015", null, null)]
    [TestCase("2015-05-29 5:50", "2015-05-29", null, null)]
    [TestCase("05/29/2015 5:50 AM", "05/29/2015", null, null)]
    [TestCase("05/29/2015 05:50:06", "05/29/2015", null, null)]
    [TestCase("May-29", "May-29", null, null)]
    [TestCase("Jul-29th", "Jul-29th", null, null)]
    [TestCase("July-1st", "July-1st", null, null)]
    [TestCase("2015-05-16T05:50:06.7199222-04:00", "2015-05-16T", null, null)]
    [TestCase("2015-05-16T05:50:06", "2015-05-16T", null, null)]
    [TestCase("Fri, 16 May 2015 05:50:06 GMT", "16 May", "May 2015", null)]
    //[TestCase("05:50", "05:50", null, null)]
    //[TestCase("5:50 AM", "5:50 AM", null, null)]
    //[TestCase("05:50", "05:50", null, null)]
    //[TestCase("5:50 AM", "5:50 AM", null, null)]
    //[TestCase("05:50:06", "05:50:06", null, null)]
    [TestCase("2015 May", "2015 May", null, null)]
    //[TestCase("AB 13:10", "13:10", null, null)]
    public void IsIdentifiable_TestDates(string date, string expectedMatch1, string? expectedMatch2, string? expectedMatch3)
    {
        var runner = new TestRunner($"Patient next appointment is {date}", _fileSystem);
        runner.Run();

        Assert.Multiple(() =>
        {
            Assert.That(runner.ResultsOfValidate[0].Word, Is.EqualTo(expectedMatch1));
            Assert.That(runner.ResultsOfValidate[0].Classification, Is.EqualTo(FailureClassification.Date));
        });

        if (expectedMatch2 != null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(runner.ResultsOfValidate[1].Word, Is.EqualTo(expectedMatch2));
                Assert.That(runner.ResultsOfValidate[1].Classification, Is.EqualTo(FailureClassification.Date));
            });
        }
        if (expectedMatch3 != null)
        {
            Assert.Multiple(() =>
            {
                Assert.That(runner.ResultsOfValidate[2].Word, Is.EqualTo(expectedMatch3));
                Assert.That(runner.ResultsOfValidate[2].Classification, Is.EqualTo(FailureClassification.Date));
            });
        }
    }

    [TestCase("We are going to the pub on Friday at about 3'o clock")]
    [TestCase("We may go there in August some time")]
    [TestCase("I will be 30 in September")]
    [TestCase("Prescribed volume is is 32.0 ml")]
    [TestCase("2001.1.2")]
    [TestCase("AB13:10")]
    public void IsIdentifiable_Test_NotADate(string input)
    {
        var runner = new TestRunner(input, _fileSystem);
        runner.Run();

        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    [Test]
    public void TestChiAndNameInString()
    {
        var runner = new TestRunner("David Smith should be referred to with chi 0101010101", _fileSystem);

        runner.Run();
        Assert.That(runner.ResultsOfValidate, Has.Count.EqualTo(1));

        var w1 = runner.ResultsOfValidate[0];

        Assert.Multiple(() =>
        {
            /* Names are now picked up by the Socket NER Daemon
   //FailurePart w2 = runner.ResultsOfValidate[1];
   //FailurePart w3 = runner.ResultsOfValidate[2];


   Assert.AreEqual("David", w1.Word);
   Assert.AreEqual(0, w1.Offset);

   Assert.AreEqual("Smith", w2.Word);
   Assert.AreEqual(6, w2.Offset);
   */

            Assert.That(w1.Word, Is.EqualTo("0101010101"));
            Assert.That(w1.Offset, Is.EqualTo(43));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestCaseSensitivity_BlackBox(bool caseSensitive)
    {
        var runner = new TestRunner("FF", _fileSystem);

        runner.CustomRules.Add(new RegexRule()
        {
            IfPattern = "ff",
            Action = RuleAction.Ignore,
            CaseSensitive = caseSensitive
        });

        runner.CustomRules.Add(new RegexRule() { IfPattern = "\\w+", Action = RuleAction.Report, As = FailureClassification.Person });

        runner.Run();

        if (caseSensitive)
            Assert.That(runner.ResultsOfValidate, Has.Count.EqualTo(1));
        else
            Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    /// <summary>
    /// This tests that the rule order is irrelevant.  Ignore rules should always be applied before report rules
    /// </summary>
    /// <param name="ignoreFirst"></param>
    [TestCase(true)]
    [TestCase(false)]
    public void TestRuleOrdering_BlackBox(bool ignoreFirst)
    {
        var runner = new TestRunner("FF", _fileSystem);

        if (ignoreFirst)
        {
            //ignore the report
            runner.CustomRules.Add(new RegexRule { IfPattern = "FF", Action = RuleAction.Ignore });
            runner.CustomRules.Add(new RegexRule() { IfPattern = "\\w+", Action = RuleAction.Report, As = FailureClassification.Person });
        }
        else
        {
            //report then ignore
            runner.CustomRules.Add(new RegexRule() { IfPattern = "\\w+", Action = RuleAction.Report, As = FailureClassification.Person });
            runner.CustomRules.Add(new RegexRule { IfPattern = "FF", Action = RuleAction.Ignore });
        }

        runner.SortRules();

        runner.Run();

        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    [Test]
    public void TestSopDoesNotMatch()
    {
        const string sopKey = "SOPInstanceUID";
        const string exampleSop = "1.2.392.200036.9116.2.6.1.48.1214834115.1486205112.923825";
        var testOpts = new TestOpts
        {
            SkipColumns = sopKey
        };

        var runner = new TestRunner(exampleSop, testOpts, _fileSystem, sopKey);

        runner.Run();
        Assert.That(runner.ResultsOfValidate, Is.Empty);
    }

    [Test]
    public void TestEmptyRulesDir()
    {
        var emptyDir = "empty";
        _fileSystem.Directory.CreateDirectory(emptyDir);

        Assert.That(_fileSystem.Directory.GetFiles(emptyDir, "*.yaml"), Is.Empty, $"Expected the empty dir not to have any rules yaml files");

        var testOpts = new TestOpts
        {
            RulesDirectory = emptyDir
        };

        var ex = Assert.Throws<Exception>(() => new TestRunner("fff", testOpts, _fileSystem));
        Assert.That(ex.Message, Does.Contain(" did not contain any rules files containing rules"));
    }
    [Test]
    public void TestMissingRulesDir()
    {
        var missingDir = "hahaIdontexist";

        var testOpts = new TestOpts
        {
            RulesDirectory = missingDir
        };

        var ex = Assert.Throws<System.IO.DirectoryNotFoundException>(() => new TestRunner("fff", testOpts, _fileSystem));
        Assert.That(ex.Message, Does.Contain("Could not find a part of the path"));
        Assert.That(ex.Message, Does.Contain("hahaIdontexist"));
    }

    [TestCase("#this is an empty yaml file with no rules")]
    [TestCase("SocketRules:")]
    public void TestOnlyEmptyRulesFilesInDir(string yaml)
    {
        var emptyishDir = "emptyish";
        _fileSystem.Directory.CreateDirectory(emptyishDir);

        var rulePath = _fileSystem.Path.Combine(emptyishDir, "Somerules.yaml");

        //notice that this file is empty
        _fileSystem.File.WriteAllText(rulePath, yaml);

        Assert.That(_fileSystem.Directory.GetFiles(emptyishDir, "*.yaml"), Is.Not.Empty);

        var testOpts = new TestOpts
        {
            RulesDirectory = emptyishDir
        };

        var ex = Assert.Throws<Exception>(() => new TestRunner("fff", testOpts, _fileSystem));
        Assert.That(ex.Message, Does.Contain(" did not contain any rules files containing rules"));
    }



    private class TestRunner : IsIdentifiableAbstractRunner
    {
        public string FieldToTest { get; set; }
        public string ValueToTest { get; set; }

        public readonly List<FailurePart> ResultsOfValidate = new();

        public TestRunner(string valueToTest, MockFileSystem fileSystem)
            : base(new TestOpts(), fileSystem)
        {
            ValueToTest = valueToTest;
            FieldToTest = "field";
        }

        public TestRunner(string valueToTest, TestOpts opts, MockFileSystem fileSystem, string fieldToTest = "field")
            : base(opts, fileSystem)
        {
            FieldToTest = fieldToTest;
            ValueToTest = valueToTest;
        }

        public override int Run()
        {
            ResultsOfValidate.AddRange(Validate(FieldToTest, ValueToTest).OrderBy(v => v.Offset));
            CloseReports();
            return 0;
        }
    }

    private class TestOpts : IsIdentifiableOptions
    {
        public TestOpts()
        {
            DestinationCsvFolder = "";
            StoreReport = true;
        }
        public override string GetTargetName(System.IO.Abstractions.IFileSystem _)
        {
            // avoids collisions where multiple output reports are attempted at the same second
            return Guid.NewGuid().ToString();
        }
    }
}
