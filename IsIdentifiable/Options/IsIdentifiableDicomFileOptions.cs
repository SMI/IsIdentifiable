using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace IsIdentifiable.Options;

/// <summary>
/// Options class for running IsIdentifiable on all dicom files in a given directory
/// </summary>
[Verb("dir", HelpText = "Run tool on one or more dicom files and evaluate tag contents")]
public class IsIdentifiableDicomFileOptions : IsIdentifiableOptions
{
    /// <summary>
    /// Directory in which to recursively search for dicom files
    /// </summary>
    [Option('d', HelpText = "Directory in which to recursively search for dicom files", Required = true)]
    public string Directory { get; set; }

    /// <summary>
    /// Optional. Search pattern for files, defaults to *.dcm)
    /// </summary>
    [Option(HelpText = "Optional. Search pattern for files, defaults to *.dcm)", Default = "*.dcm")]
    public string Pattern { get; set; }

    /// <summary>
    /// Optional. True to check the files opened have a valid dicom preamble
    /// </summary>
    [Option(HelpText = "Optional. True to check the files opened have a valid dicom preamble", Default = true)]
    public bool RequirePreamble { get; set; }

    /// <summary>
    /// Optional. Path to a 'tessdata' directory.  tessdata.eng must exist here.  If specified then the DICOM file's pixel data will be run through text detection
    /// </summary>
    [Option(HelpText = "Optional. Path to a 'tessdata' directory.  tessdata.eng must exist here.  If specified then the DICOM file's pixel data will be run through text detection")]
    public string TessDirectory { get; set; }

    /// <summary>
    /// Optional. If set images will be rotated to 90, 180 and 270 degrees (clockwise) to allow OCR to pick up upside down or horizontal text.
    /// </summary>
    [Option(HelpText = "Optional. If set images will be rotated to 90, 180 and 270 degrees (clockwise) to allow OCR to pick up upside down or horizontal text.")]
    public bool Rotate { get; set; }

    /// <summary>
    /// Optional.  If set any image tag which contains a DateTime will result in a failure
    /// </summary>
    [Option(HelpText = "Optional.  If set any image tag which contains a DateTime will result in a failure")]
    public bool NoDateFields { get; set; }

    /// <summary>
    /// Optional.  If NoDateFields is set then this value will not result in a failure.  e.g. 0001-01-01
    /// </summary>
    [Option(HelpText = "Optional.  If NoDateFields is set then this value will not result in a failure.  e.g. 0001-01-01")]
    public string ZeroDate { get; set; }

    /// <summary>
    /// Optional. If non-zero, will ignore any reported pixel data text less than (but not equal to) the specified number of characters
    /// </summary>
    [Option(HelpText = "Optional. If non-zero, will ignore any reported pixel data text less than (but not equal to) the specified number of characters")]
    public uint IgnoreTextLessThan { get; set; } = 0;

    /// <summary>
    /// Usage examples for running IsIdentifiable on dicom files
    /// </summary>
    [Usage]
    public static IEnumerable<Example> Examples
    {
        get
        {
            yield return new Example("Classify all *.dcm files and run the pixel text classifier.  This example works if the 'tessdata' is in the current directory",
                new IsIdentifiableDicomFileOptions
                {
                    Directory = @"C:\MyDataFolder",
                    TessDirectory = ".",
                    StoreReport = true
                });

            yield return new Example("Attempt to interpret all files as DICOM",
                new IsIdentifiableDicomFileOptions
                {
                    Directory = @"C:\MyDataFolder",
                    Pattern = "*",
                    StoreReport = true
                });
        }
    }

    /// <summary>
    /// Returns the name of the root directory being evaluated
    /// </summary>
    /// <returns></returns>
    public override string GetTargetName(IFileSystem fileSystem)
    {
        return Directory == null ? "No Directory Specified" : fileSystem.DirectoryInfo.New(Directory).Name;
    }

    /// <summary>
    /// Checks that the options specified are compatible.  Throws if they are not.
    /// </summary>
    /// <exception cref="Exception">Thrown if options are incompatible</exception>
    public override void ValidateOptions()
    {
        base.ValidateOptions();

        if (string.IsNullOrWhiteSpace(TessDirectory) && Rotate)
            throw new Exception("Rotate option is only valid if OCR is running (TessDirectory is set)");

        if (!string.IsNullOrWhiteSpace(ZeroDate) && !NoDateFields)
            throw new Exception("ZeroDate is only valid if the NoDateFields flag is set");
    }
}
