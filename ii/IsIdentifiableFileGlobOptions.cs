using CommandLine;
using IsIdentifiable.Options;
using System.IO.Abstractions;

namespace ii;

/// <summary>
/// Overrides <see cref="IsIdentifiableFileOptions"/> to support looped running over multiple files
/// via file matching globs
/// </summary>
[Verb("file", HelpText = "Run tool on one or more delimited textual data files (e.g. csv)")]
internal class IsIdentifiableFileGlobOptions : IsIdentifiableFileOptions
{
    [Option('f', HelpText = "Path to a file or directory to be evaluated", Required = true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public new IFileInfo File { get; set; }
#pragma warning restore CS8618

    [Option('g', HelpText = "Pattern to use for matching files when -f is a directory.  Supports specifying a glob e.g. /**/*.csv", Required = false, Default = "*.csv")]
    public string Glob { get; set; } = "*.csv";
}
