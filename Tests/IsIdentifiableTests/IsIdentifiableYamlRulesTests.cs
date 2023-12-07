using IsIdentifiable.Failures;
using IsIdentifiable.Rules;
using NUnit.Framework;
using System;
using System.Linq;
using YamlDotNet.Serialization;

namespace IsIdentifiable.Tests;

class IsIdentifiableRuleTests
{
    [Test]
    public void TestYamlDeserialization_OfRules()
    {
        var yaml = @"
BasicRules: 
  # Ignore any values in the column Modality
  - Action: Ignore
    IfColumn: Modality

  # Ignore the value CT in the column Modality
  - Action: Ignore
    IfColumn: Modality
    IfPattern: ^CT$

  # Report as an error any values which contain 2 digits
  - IfPattern: ""[0-9][0-9]""
    Action: Report
    As: PrivateIdentifier

SocketRules:   
  - Host: 127.0.123.123
    Port: 8080
 ";

        var deserializer = new Deserializer();
        var ruleSet = deserializer.Deserialize<RuleSet>(yaml);

        Assert.Multiple(() =>
        {
            Assert.That(ruleSet.BasicRules, Has.Count.EqualTo(3));
            Assert.That(ruleSet.BasicRules[0].Action, Is.EqualTo(RuleAction.Ignore));

            Assert.That(ruleSet.SocketRules, Has.Count.EqualTo(1));
            Assert.That(ruleSet.SocketRules[0].Host, Is.EqualTo("127.0.123.123"));
            Assert.That(ruleSet.SocketRules[0].Port, Is.EqualTo(8080));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestOneRule_IsColumnMatch_NoPattern(bool isReport)
    {
        var rule = new RegexRule()
        {
            Action = isReport ? RuleAction.Report : RuleAction.Ignore,
            IfColumn = "Modality",
            As = FailureClassification.Date
        };

        Assert.That(rule.Apply("MODALITY", "CT", out var bad), Is.EqualTo(isReport ? RuleAction.Report : RuleAction.Ignore));

        if (isReport)
            Assert.That(bad.Single().Classification, Is.EqualTo(FailureClassification.Date));
        else
            Assert.That(bad, Is.Empty);

        Assert.That(rule.Apply("ImageType", "PRIMARY", out _), Is.EqualTo(RuleAction.None));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Test_RegexMultipleMatches(bool isReport)
    {
        var rule = new RegexRule()
        {
            Action = isReport ? RuleAction.Report : RuleAction.Ignore,
            IfColumn = "Modality",
            IfPattern = "[0-9],",
            As = FailureClassification.Date
        };

        Assert.That(rule.Apply("MODALITY", "1,2,3", out var bad), Is.EqualTo(isReport ? RuleAction.Report : RuleAction.Ignore));

        if (isReport)
        {
            Assert.Multiple(() =>
            {
                var b = bad.ToArray();

                Assert.That(b[0].Word, Is.EqualTo("1,"));
                Assert.That(b[0].Classification, Is.EqualTo(FailureClassification.Date));
                Assert.That(b[0].Offset, Is.EqualTo(0));

                Assert.That(b[1].Word, Is.EqualTo("2,"));
                Assert.That(b[1].Classification, Is.EqualTo(FailureClassification.Date));
                Assert.That(b[1].Offset, Is.EqualTo(2));
            });
        }
        else
            Assert.That(bad, Is.Empty);

        Assert.That(rule.Apply("ImageType", "PRIMARY", out _), Is.EqualTo(RuleAction.None));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestOneRule_IsColumnMatch_WithPattern(bool isReport)
    {
        var rule = new RegexRule()
        {
            Action = isReport ? RuleAction.Report : RuleAction.Ignore,
            IfColumn = "Modality",
            IfPattern = "^CT$",
            As = FailureClassification.Date
        };

        Assert.Multiple(() =>
        {
            Assert.That(rule.Apply("Modality", "CT", out _), Is.EqualTo(isReport ? RuleAction.Report : RuleAction.Ignore));
            Assert.That(rule.Apply("Modality", "MR", out _), Is.EqualTo(RuleAction.None));
            Assert.That(rule.Apply("ImageType", "PRIMARY", out _), Is.EqualTo(RuleAction.None));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestOneRule_NoColumn_WithPattern(bool isReport)
    {
        var rule = new RegexRule()
        {
            Action = isReport ? RuleAction.Report : RuleAction.Ignore,
            IfPattern = "^CT$",
            As = FailureClassification.Date
        };

        Assert.Multiple(() =>
        {
            Assert.That(rule.Apply("Modality", "CT", out _), Is.EqualTo(isReport ? RuleAction.Report : RuleAction.Ignore));
            Assert.That(rule.Apply("ImageType", "CT", out _), Is.EqualTo(isReport ? RuleAction.Report : RuleAction.Ignore)); //ignore both because no restriction on column
            Assert.That(rule.Apply("ImageType", "PRIMARY", out _), Is.EqualTo(RuleAction.None));
        });
    }

    [Test]
    public void TestAreIdentical()
    {
        var rule1 = new RegexRule();
        var rule2 = new RegexRule();

        Assert.That(rule1.AreIdentical(rule2), Is.True);

        rule2.IfPattern = "\r\n";
        Assert.That(rule1.AreIdentical(rule2), Is.False);
        rule1.IfPattern = "\r\n";
        Assert.That(rule1.AreIdentical(rule2), Is.True);


        rule2.IfColumn = "MyCol";
        Assert.That(rule1.AreIdentical(rule2), Is.False);
        rule1.IfColumn = "MyCol";
        Assert.That(rule1.AreIdentical(rule2), Is.True);

        rule2.Action = RuleAction.Ignore;
        rule1.Action = RuleAction.Report;
        Assert.Multiple(() =>
        {
            Assert.That(rule1.AreIdentical(rule2, true), Is.False);
            Assert.That(rule1.AreIdentical(rule2, false), Is.True);
        });

        rule2.Action = RuleAction.Report;
        rule1.Action = RuleAction.Report;
        Assert.Multiple(() =>
        {
            Assert.That(rule1.AreIdentical(rule2, false), Is.True);
            Assert.That(rule1.AreIdentical(rule2, true), Is.True);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestOneRule_NoColumn_NoPattern(bool isReport)
    {
        //rule is to ignore everything
        var rule = new RegexRule()
        {
            Action = isReport ? RuleAction.Report : RuleAction.Ignore,
        };

        var ex = Assert.Throws<Exception>(() => rule.Apply("Modality", "CT", out _));

        Assert.That(ex.Message, Is.EqualTo("Illegal rule setup.  You must specify either a column or a pattern (or both)"));
    }
}
