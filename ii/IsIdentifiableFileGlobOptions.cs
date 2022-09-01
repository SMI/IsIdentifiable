using CommandLine;
using IsIdentifiable.Options;

namespace IsIdentifiable;

/// <summary>
/// Overrides <see cref="IsIdentifiableFileOptions"/> to support looped running over multiple files
/// via file matching globs
/// </summary>
[Verb("file", HelpText = "Run tool on one or more delimited textual data files (e.g. csv)")]
internal class IsIdentifiableFileGlobOptions : IsIdentifiableFileOptions
{
    [Option('f', HelpText = "Path to a file or directory to be evaluated", Required = true)]
    public new string? File { get; set; }

    [Option('g', HelpText = "Pattern to use for matching files when -f is a directory.  Supports specifying a glob e.g. /**/*.csv", Required = false,Default = "*.csv")]
    public string Glob { get; set; } = "*.csv";
}
