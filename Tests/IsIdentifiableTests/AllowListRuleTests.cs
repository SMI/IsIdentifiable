using IsIdentifiable.Failures;
using IsIdentifiable.Rules;
using NUnit.Framework;

namespace IsIdentifiable.Tests;

class AllowlistRuleTests
{

    [Test]
    public void TestAllowlistRule_IfPattern_CaseSensitivity()
    {
        var rule = new AllowlistRule
        {
            //Ignore any failure where any of the input string matches "fff"
            IfPattern = "fff"
        };

        Assert.Multiple(() =>
        {
            Assert.That(rule.CaseSensitive, Is.False);

            Assert.That(
    rule.ApplyAllowlistRule("aba", "FFF Troll", new FailurePart("Troll", FailureClassification.Location, 0)), Is.EqualTo(RuleAction.Ignore));
        });

        rule.CaseSensitive = true;

        Assert.That(
rule.ApplyAllowlistRule("aba", "FFF Troll", new FailurePart("Troll", FailureClassification.Location, 0)), Is.EqualTo(RuleAction.None));
    }

    [Test]
    public void TestAllowlistRule_IfPartPattern_CaseSensitivity()
    {
        var rule = new AllowlistRule
        {
            //Ignore any failure the specific section that is bad is this:
            IfPartPattern = "^troll$"
        };

        Assert.Multiple(() =>
        {
            Assert.That(rule.CaseSensitive, Is.False);

            Assert.That(
    rule.ApplyAllowlistRule("aba", "FFF Troll", new FailurePart("Troll", FailureClassification.Location, 0)), Is.EqualTo(RuleAction.Ignore));
        });

        rule.CaseSensitive = true;

        Assert.That(
rule.ApplyAllowlistRule("aba", "FFF Troll", new FailurePart("Troll", FailureClassification.Location, 0)), Is.EqualTo(RuleAction.None));
    }

    [Test]
    public void TestAllowlistRule_As()
    {
        var rule = new AllowlistRule
        {
            //Ignore any failure the specific section that is bad is this:
            IfPartPattern = "^troll$",
            As = FailureClassification.Person
        };

        Assert.Multiple(() =>
        {
            Assert.That(
    rule.ApplyAllowlistRule("aba", "FFF Troll",
                    new FailurePart("Troll", FailureClassification.Location, 0)), Is.EqualTo(RuleAction.None), "Rule should not apply when FailureClassification is Location");

            Assert.That(
    rule.ApplyAllowlistRule("aba", "FFF Troll",
                    new FailurePart("Troll", FailureClassification.Person, 0)), Is.EqualTo(RuleAction.Ignore), "Rule SHOULD apply when FailureClassification matches As");
        });

    }

    [Test]
    public void TestCombiningPatternAndPart()
    {
        var rule = new AllowlistRule()
        {
            IfPartPattern = "^Brian$",
            IfPattern = "^MR Brian And Skull$"
        };

        Assert.Multiple(() =>
        {
            Assert.That(
        rule.ApplyAllowlistRule("aba", "MR Brian And Skull",
                        new FailurePart("Brian", FailureClassification.Person, 0)), Is.EqualTo(RuleAction.Ignore), "Rule matches on both patterns");

            Assert.That(
    rule.ApplyAllowlistRule("aba", "MR Brian And Skull",
                    new FailurePart("Skull", FailureClassification.Person, 0)), Is.EqualTo(RuleAction.None), "Rule does not match on both whole string AND part so should not be ignored");

            Assert.That(
    rule.ApplyAllowlistRule("aba", "MR Brian And Skull Dr Fisher",
                    new FailurePart("Brian", FailureClassification.Person, 0)), Is.EqualTo(RuleAction.None), "Rule does not match on both whole string AND part so should not be ignored");
        });
    }


}
