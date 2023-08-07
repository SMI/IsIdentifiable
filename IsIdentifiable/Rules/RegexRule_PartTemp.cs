using IsIdentifiable.Failures;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace IsIdentifiable.Rules;

[Obsolete("Temporary workaround -- do not use")]
public class PartRegexRule_Temp : RegexRule
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

    // TODO(rkm 2023-07-25) Shouldn't be needed when IfPattern is readonly
    private void RebuildPartRegex()
    {
        if (!_ifPartPatternString.StartsWith("^") || _ifPartPatternString.EndsWith("$"))
        if (!_ifPartPatternString.StartsWith("^") || !_ifPartPatternString.EndsWith("$"))
            throw new ArgumentException("IfPartPattern must be enclosed by ^ and $");
        IfPartPatternRegex = _ifPartPatternString == null ? null : new Regex(_ifPartPatternString, (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Compiled);
    }

    public bool Covers(FailurePart failurePart)
    {
        if (IfPartPattern == null)
            throw new Exception("Illegal rule setup. You must specify IfPartPattern");

        if (As != failurePart.Classification)
            return false;

        var matches = IfPartPatternRegex.Matches(failurePart.Word);
        return matches.Any();
    }
}
