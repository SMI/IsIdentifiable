using IsIdentifiable.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// XXX using RegexOptions.Compiled may result in a large amount of static code
// which is never freed during garbage collection, see
// https://docs.microsoft.com/en-us/dotnet/standard/base-types/compilation-and-reuse-in-regular-expressions
// Note that the Regex Cache is not used in instance methods.

namespace IsIdentifiable.Rules;

/// <summary>
/// Expanded <see cref="RegexRule"/> which works only for <see cref="RuleAction.Ignore"/>.  Should be run after main rules have picked up failures.  This class is designed to perform final checks on failures and discard based on <see cref="RegexRule.IfPatternRegex"/> and/or <see cref="IfPartPatternRegex"/>
/// </summary>
public record AllowlistRule : RegexRule
{
    /// <inheritdoc />
    public virtual bool Equals(AllowlistRule? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && _ifPartPatternString == other._ifPartPatternString;
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _ifPartPatternString);

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

    /// <summary>
    /// Creates a new instance with the <see cref="Action"/> Ignore
    /// </summary>
    public AllowlistRule()
    {
        Action = RuleAction.Ignore;
    }

    private void RebuildPartRegex()
    {
        IfPartPatternRegex = _ifPartPatternString == null ? null : new Regex(_ifPartPatternString, (CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Compiled);
    }

    /// <summary>
    /// A fake method due to inheriting IAppliableRule; never called.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public override RuleAction Apply(string fieldName, string fieldValue, out List<FailurePart> badParts)
    {
        throw new NotSupportedException("This method should not be used for AllowlistRule, use ApplyAllowlistRule instead");
    }


    /// <summary>
    /// Test if this rule will Allowlist the given field and failed part.
    /// Returns None if rule does not match ALL constraints otherwise returns the rule action
    /// which should be Ignore for a Allowlist rule as Report is already true at this point.
    /// </summary>
    public RuleAction ApplyAllowlistRule(string fieldName, string fieldValue, FailurePart badPart)
    {
        // FailurePart has got Classification, Word and Offset
        // (we can't access ProblemField, ProblemValue from Failure).
        // eg. we get:
        //   fieldName=ProtocolName fieldValue=324-58-2995/6 part.Word=324-58-2995 class=Location
        //Console.WriteLine("AllowlistRule.Apply fieldName="+ fieldName + " fieldValue=" + fieldValue + " part.Word=" + badPart.Word + " class="+badPart.Classification);

        if (Action == RuleAction.Report)
            throw new Exception("Illegal Allowlist rule setup. Action Report makes no sense.");

        // A column or field name is specified
        if (!string.IsNullOrWhiteSpace(IfColumn) && !string.Equals(IfColumn, fieldName, StringComparison.InvariantCultureIgnoreCase))
            return RuleAction.None;

        // A failure classification specified (eg. a Location or a Person)
        if ((As != FailureClassification.None) && (As != badPart.Classification))
            return (RuleAction.None);

        // A pattern to match the specific part (substring) which previously failed
        if (IfPartPatternRegex != null && !IfPartPatternRegex.Matches(badPart.Word).Any())
            return (RuleAction.None);

        // A pattern to match the whole of the field value, not just the bit which failed
        if (IfPatternRegex != null && !IfPatternRegex.Matches(fieldValue).Any())
            return (RuleAction.None);

        /*_logger.Debug("Allowlisting fieldName: "+ fieldName + " fieldValue: " + fieldValue + " part.Word: " + badPart.Word + " class: "+badPart.Classification
        + " due to rule: "
        + (_ifPattern == null ? "" : "Pattern: " + _ifPattern.ToString())
        + (IfPartPattern == null ? "" : " Part: " + IfPartPattern.ToString())
        + (IfColumn == null ? "" : " Column: " + IfColumn)
        + (IfClassification == FailureClassification.None ? "" : " Classification: " + IfClassification));*/

        return Action;
    }
}
