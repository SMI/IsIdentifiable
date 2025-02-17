using IsIdentifiable.Failures;
using System.Collections.Generic;

namespace IsIdentifiable.Rules;

/// <summary>
/// A rule which may Allowlist or report as a validation failure a given value
/// during IsIdentifiable analysis
/// </summary>
public interface IAppliableRule
{
    /// <summary>
    /// Applies the rule to the current value (being validated).
    /// </summary>
    /// <param name="fieldName">The column name or dicom tag keyword that is being validated e.g. "PatientID"</param>
    /// <param name="fieldValue">The value that should be validated e.g. "0101010101"</param>
    /// <param name="badParts"></param>
    /// <returns>Action to take if any as a result of applying the rule</returns>
    RuleAction Apply(string fieldName, string fieldValue, out List<FailurePart>? badParts);
}
