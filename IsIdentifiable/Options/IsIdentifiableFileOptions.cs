using CommandLine;
using System.IO.Abstractions;

namespace IsIdentifiable.Options;

/// <summary>
/// Options class for when running IsIdentifiable on a text data file e.g. csv
/// </summary>
[Verb("file", HelpText = "Run tool on delimited textual data file e.g. csv")]
public class IsIdentifiableFileOptions : IsIdentifiableBaseOptions
{
    /// <summary>
    /// Path to a file to be evaluated
    /// </summary>

    [Option('f', HelpText = "Path to a file to be evaluated", Required = true)]
    public IFileInfo File { get; set; }

    /// <summary>
    /// Optional.  The culture of dates, numbers etc if different from system culture
    /// </summary>
    [Option('c', HelpText = "The culture of dates, numbers etc if different from system culture")]
    public string Culture { get; set; }

    /// <summary>
    /// Returns the name of the <see cref="File"/> (for use in outputted report names)
    /// </summary>
    /// <returns></returns>
    public override string GetTargetName(IFileSystem _)
    {
        return File.Name;
    }
}