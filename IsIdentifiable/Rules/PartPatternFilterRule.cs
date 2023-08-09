using IsIdentifiable.Failures;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IsIdentifiable.Rules;

public class PartPatternFilterRule : RegexRule
{
    /// <summary>
    /// Combination of <see cref="IfPartPattern"/> and <see cref="CaseSensitive"/>.  Use this to validate
    /// whether the rule should be applied.
    /// </summary>
    protected Regex IfPartPatternRegex;
    private string _ifPartPatternString;

    /// <summary>
    /// The Regex pattern which should be used to match values a specific failing part
    /// </summary>
    public string IfPartPattern
    {
        get => _ifPartPatternString;
        set
        {
            _ifPartPatternString = value;
            RebuildPartRegex();
        }
    }

    /// <summary>
    /// Whether the IfPattern and IfPartPattern are case sensitive (default is false)
    /// </summary>
    public override bool CaseSensitive
    {
        get => base.CaseSensitive;
        set
        {
            base.CaseSensitive = value;
            RebuildPartRegex();
        }
    }

    public string WordBefore { get; set; }

    public string WordAfter { get; set; }

    // TODO(rkm 2023-07-25) Shouldn't be needed when IfPattern is readonly
    private void RebuildPartRegex()
    {
        if (_ifPartPatternString == null)
            throw new Exception("Illegal rule setup. You must specify IfPartPattern");

        if (!_ifPartPatternString.StartsWith("^") || !_ifPartPatternString.EndsWith("$"))
            throw new ArgumentException("IfPartPattern must be enclosed by ^ and $");

        IfPartPatternRegex = new Regex(_ifPartPatternString, (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Compiled);
    }

    public bool Covers(FailurePart failurePart, string problemValue)
    {
        if (As != failurePart.Classification)
            return false;

        bool matchesBefore = false;
        if (!string.IsNullOrWhiteSpace(WordBefore))
        {
            var problemValueUpToOffset = problemValue[..(failurePart.Offset + failurePart.Word.Length)];
            var wordBeforeRegex = new Regex($"\\b{WordBefore}\\s+{IfPartPattern.TrimStart('^')}", (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
            matchesBefore = wordBeforeRegex.Matches(problemValueUpToOffset).Any();
        }

        bool matchesAfter = false;
        if (!string.IsNullOrWhiteSpace(WordAfter))
        {
            var problemValueFromOffset = problemValue[failurePart.Offset..];
            var wordAfterRegex = new Regex($"{IfPartPattern.TrimEnd('$')}\\s+{WordAfter}\\b", (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase));
            matchesAfter = wordAfterRegex.Matches(problemValueFromOffset).Any();
        }

        if (
            matchesBefore && string.IsNullOrWhiteSpace(WordAfter) ||
            matchesAfter && string.IsNullOrWhiteSpace(WordBefore) ||
            (matchesBefore && matchesAfter)
        )
        {
            return true;
        }
        else if (!string.IsNullOrWhiteSpace(WordBefore) || !string.IsNullOrWhiteSpace(WordAfter))
        {
            return false;
        }

        return IfPartPatternRegex.Matches(failurePart.Word).Any();
    }
}
