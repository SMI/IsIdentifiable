using System;
using IsIdentifiable.Options;
using System.Data;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace IsIdentifiable.Reporting.Destinations;

/// <summary>
/// Abstract implementation of <see cref="IReportDestination"/>.  When implemented
/// in a derrived class allows persistence of IsIdentifiable reports (e.g. to a
/// CSV file or database table).
/// </summary>
public abstract partial class ReportDestination : IReportDestination
{
    /// <summary>
    /// The options used to run IsIdentifiable
    /// </summary>
    protected IsIdentifiableOptions Options { get; }

    /// <summary>
    /// FileSystem to use for I/O
    /// </summary>
    protected readonly IFileSystem FileSystem;

    private readonly Regex _multiSpaceRegex = MultispaceRegex();

    /// <summary>
    /// Initializes the report destination and sets <see cref="Options"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="fileSystem"></param>
    protected ReportDestination(IsIdentifiableOptions options, IFileSystem fileSystem)
    {
        Options = options;
        FileSystem = fileSystem;
    }

    /// <summary>
    /// Override to output the column <paramref name="headers"/> e.g. as the first line of a CSV
    /// </summary>
    /// <param name="headers"></param>
    public virtual void WriteHeader(params string[] headers) { }

    /// <summary>
    /// Override to output the given <paramref name="batch"/> of rows.  Column names will match
    /// <see cref="WriteHeader(string[])"/>.  Each row contains report data that must be persisted
    /// </summary>
    /// <param name="batch"></param>
    public abstract void WriteItems(DataTable batch);

    /// <summary>
    /// Override to perform any tidyup on the destination e.g. close file handles / end transactions
    /// </summary>
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns <paramref name="o"/> with whitespace stripped (if it is a string and <see cref="IsIdentifiableOptions.DestinationNoWhitespace"/>
    /// is set on command line options).
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    protected object StripWhitespace(object o)
    {
        if (o is string s && Options.DestinationNoWhitespace)
            return _multiSpaceRegex.Replace(s.Replace("\t", "").Replace("\r", "").Replace("\n", ""), " ");

        return o;
    }

    [GeneratedRegex(" {2,}", RegexOptions.CultureInvariant)]
    private static partial Regex MultispaceRegex();
}
