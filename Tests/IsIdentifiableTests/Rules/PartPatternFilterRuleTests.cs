using IsIdentifiable.Failures;
using IsIdentifiable.Rules;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace IsIdentifiable.Tests.Rules;

internal class PartPatternFilterRuleTests
{
    private static IEnumerable<string> TestCaseSource_ForamenMonroParts()
    {
        var parts = new List<string>();
        foreach (var prefix in new[] { "foramen", "foramina" })
        {
            foreach (var join in new[] { "of", "" })
            {
                foreach (var name in new[] { "monro", "monroe" })
                {
                    parts.Add(string.Join(" ", (new[] { prefix, join, name }).Where(x => !string.IsNullOrEmpty(x))));
                }
            }
        }
        return parts;
    }

    [TestCaseSource(nameof(TestCaseSource_ForamenMonroParts))]
    public void ForamenMonro(string valuePart)
    {
        // Arrange
        var rule = new PartPatternFilterRule()
        {
            IfPartPattern = "^Monroe?$",
            WordBefore = "(foramen|foramina)( of)?",
            IfColumn = "TextValue",
            As = FailureClassification.Person,
            Action = RuleAction.Ignore,
        };
        var name = valuePart.Split()[^1];
        var problemValue = $"Mr {name} has an issue with his {valuePart}";
        var validFailurePart = new FailurePart(name, FailureClassification.Person, 3);
        var problemOffset = problemValue.LastIndexOf(" ") + 1;
        var filteredFailurePart = new FailurePart(name, FailureClassification.Person, problemOffset);

        // Act
        var coversValidFailurePart = rule.Covers(validFailurePart, problemValue);
        var coversFilteredFailurePart = rule.Covers(filteredFailurePart, problemValue);

        // Assert
        Assert.False(coversValidFailurePart);
        Assert.True(coversFilteredFailurePart);
    }

    private static IEnumerable<string> TestCaseSource_HodgkinLymphomaParts()
    {
        var parts = new List<string>();
        foreach (var name in new[] { "hodgkin", "hodgkins", "hodgkin's" })
        {
            foreach (var postfix in new[] { "lymphoma", "disease" })
            {
                parts.Add(string.Join(" ", (new[] { name, postfix }).Where(x => !string.IsNullOrEmpty(x))));
            }
        }
        return parts;
    }

    [TestCaseSource(nameof(TestCaseSource_HodgkinLymphomaParts))]
    public void HodgkinLymphoma(string valuePart)
    {
        // Arrange
        var rule = new PartPatternFilterRule()
        {
            Action = RuleAction.Ignore,
            As = FailureClassification.Person,
            IfColumn = "TextValue",
            IfPartPattern = "^Hodgkin(s|'s)?$",
            WordAfter = "(lymphoma|disease|<br>lymphoma)",
        };
        var name = valuePart.Split()[0];
        var problemValue = $"Mr {name} possibly has {valuePart}";
        var validFailurePart = new FailurePart(name, FailureClassification.Person, 3);
        var problemOffset = problemValue.IndexOf($"has {name}") + 4;
        var filteredFailurePart = new FailurePart(name, FailureClassification.Person, problemOffset);

        // Act
        var coversValidFailurePart = rule.Covers(validFailurePart, problemValue);
        var coversFilteredFailurePart = rule.Covers(filteredFailurePart, problemValue);

        // Assert
        Assert.False(coversValidFailurePart);
        Assert.True(coversFilteredFailurePart);
    }

    [TestCase]
    public void Hyphen_WordBefore()
    {
        // Arrange
        var rule = new PartPatternFilterRule()
        {
            IfPartPattern = "^Hodgkin$",
            WordBefore = "Non",
            IfColumn = "TextValue",
            As = FailureClassification.Person,
            Action = RuleAction.Ignore,
        };
        var problemValue = $"Non-Hodgkin's lymphoma";
        var failurePart = new FailurePart("Hodgkin", FailureClassification.Person, 4);

        // Act
        var ruleCoversFailurePart = rule.Covers(failurePart, problemValue);

        // Assert
        Assert.True(ruleCoversFailurePart);
    }

    [TestCase]
    public void Hyphen_WordAfter()
    {
        // Arrange
        var rule = new PartPatternFilterRule()
        {
            IfPartPattern = "^Gr(a|e)y$",
            WordAfter = "white",
            IfColumn = "TextValue",
            As = FailureClassification.Person,
            Action = RuleAction.Ignore,
        };
        var problemValue = $"Gray-white foo";
        var failurePart = new FailurePart("Gray", FailureClassification.Person, 0);

        // Act
        var ruleCoversFailurePart = rule.Covers(failurePart, problemValue);

        // Assert
        Assert.True(ruleCoversFailurePart);
    }

}
