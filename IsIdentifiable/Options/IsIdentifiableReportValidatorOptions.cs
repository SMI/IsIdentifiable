using CommandLine;
using IsIdentifiable.Reporting.Reports;
using System;

namespace IsIdentifiable.Options;

/// <summary>
/// CLI options for the validator
/// </summary>
[Verb("validate", HelpText = "Validate a FailureStoreReport")]
public class IsIdentifiableReportValidatorOptions
{
    /// <summary>
    /// The CSV list of failures to process.  Must be in the format of a <see cref="FailureStoreReport"/>
    /// </summary>
    [Option('f', "file",
        Required = true,
        HelpText = "Pre load an existing failures file"
    )]
    public string FailuresCsv { get; set; }

    /// <summary>
    /// Sets UseSystemConsole to true for Terminal.gui (i.e. uses the NetDriver which is based on System.Console)
    /// </summary>
    [Option("usc", HelpText = "Sets UseSystemConsole to true for Terminal.gui (i.e. uses the NetDriver which is based on System.Console)")]
    public bool UseSystemConsole { get; internal set; }

    /// <summary>
    /// Sets the user interface to use a specific color palette yaml file
    /// </summary>
    [Option("theme", HelpText = "Sets the user interface to use a specific color palette yaml file")]
    public string Theme { get; set; }

    /// <summary>
    /// Stop after the first error encountered
    /// </summary>
    [Option("stop-at-first-error", Required = false, Default = false, HelpText = "Stop after the first error encountered")]
    public bool StopAtFirstError { get; set; }


    /// <summary>
    /// Populates values in this instance where no value yet exists and there is a value in <paramref name="globalOpts"/>
    /// to inherit.
    /// </summary>
    /// <param name="globalOpts"></param>
    public virtual void InheritValuesFrom(IsIdentifiableReviewerOptions globalOpts)
    {
        ArgumentNullException.ThrowIfNull(globalOpts);

        if (Theme == null && !string.IsNullOrWhiteSpace(globalOpts.Theme))
            Theme = globalOpts.Theme;
    }
}
