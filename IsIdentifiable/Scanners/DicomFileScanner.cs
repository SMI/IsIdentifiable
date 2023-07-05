using DicomTypeTranslation;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using ImageMagick;
using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting.Reports;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;

namespace IsIdentifiable.Scanners;

/// <summary>
/// IsIdentifiable runner class for evaluating dicom files in a given root directory.
/// Supports recursively locating all dicom files in a directory tree.  Supports OCR
/// as well as dicom tag evaluation.  The primary key for <see cref="Failure"/> instances
/// generated by this class is the SOPInstanceUID
/// </summary>
public class DicomFileScanner : ResourceScannerBase, IFileScanner
{
    private readonly TesseractEngine? _tesseractEngine;
    private readonly PixelTextFailureReport? _tesseractReport;

    private readonly DateTime? _excludedDate = null;

    /// <summary>
    /// String used for <see cref="Failure.ProblemField"/> when reporting that the failure
    /// is based on optical character recognition on DICOM pixel data.  May include a suffix
    /// e.g. where found text is detected in a rotated image
    /// </summary>
    public const string PixelData = "PixelData";

    private readonly int _ignoreTextLessThan;
    private readonly bool _datesAreFailures;
    private readonly bool _rotate;

    /// <summary>
    /// Creates a new instance based on the <paramref name="options"/>
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public DicomFileScanner(DicomFileScannerOptions options, IFileSystem fileSystem)
        : base(options, fileSystem)
    {
        _ignoreTextLessThan = options.IgnoreTextLessThan;
        _datesAreFailures = options.DatesAreFailures;
        _rotate = options.Rotate;

        //if using Efferent.Native DICOM codecs
        // (see https://github.com/Efferent-Health/Dicom-native)
        //Dicom.Imaging.Codec.TranscoderManager.SetImplementation(new Efferent.Native.Codec.NativeTranscoderManager());

        //OR if using fo-dicom.Native DICOM codecs
        // (see https://github.com/fo-dicom/fo-dicom/issues/631)
        // Don't use WinForms, that makes us Windows-only! ImageManager.SetImplementation(new WinFormsImageManager()); 
        new DicomSetupBuilder().RegisterServices(s => s.AddImageManager<ImageSharpImageManager>()).Build();

        //if there is a value we are treating as a zero date
        if (options.DatesAreFailures && !string.IsNullOrWhiteSpace(options.ExcludedDate))
            _excludedDate = DateTime.Parse(options.ExcludedDate);

        // TODO(rkm 2023-07-05) switch this on whether pixel report specified? Inject via AddReportImpl etc.
        if (options.RunOCR)
        {
            var dir = FileSystem.DirectoryInfo.New(options.TessDirectory);
            if (!dir.Exists)
                throw new System.IO.DirectoryNotFoundException($"Could not find TESS directory '{dir}'");

            //to work with Tesseract eng.traineddata has to be in a folder called tessdata
            if (!dir.Name.Equals("tessdata"))
                dir = dir.CreateSubdirectory("tessdata");

            var languageFile = FileSystem.FileInfo.New(FileSystem.Path.Combine(dir.FullName, "eng.traineddata"));

            if (!languageFile.Exists)
                throw new System.IO.FileNotFoundException($"Could not find tesseract models file ('{languageFile.FullName}')", languageFile.FullName);

            TesseractLinuxLoaderFix.Patch();
            _tesseractEngine = new TesseractEngine(dir.FullName, "eng", EngineMode.Default)
            {
                DefaultPageSegMode = PageSegMode.Auto
            };

            // TODO(rkm 2023-07-05) Refactor out reports
            // Directory == null ? "No Directory Specified" : fileSystem.DirectoryInfo.New(Directory).Name;
            //_tesseractReport = new PixelTextFailureReport(_options.GetTargetName(FileSystem), FileSystem);
            //_reports.Add(_tesseractReport);
        }
    }

    /// <inheritdoc/>
    public void Scan(IFileInfo fi)
    {
        Logger.Debug($"Opening File: {fi.Name}");

        if (!DicomFile.HasValidHeader(fi.FullName))
            throw new ApplicationException($"File does not contain valid preamble and header: {fi.FullName}");

        var dicomFile = DicomFile.Open(fi.FullName);
        var dataSet = dicomFile.Dataset;

        if (_tesseractEngine != null)
            ValidateDicomPixelData(fi, dicomFile, dataSet);

        foreach (var dicomItem in dataSet)
            ValidateDicomItem(fi, dicomFile, dataSet, dicomItem);

        NotifyDoneRows(1);
    }

    private void ValidateDicomItem(IFileInfo fi, DicomFile dicomFile, DicomDataset dataset, DicomItem dicomItem)
    {
        //if it is a sequence get the Sequences dataset and then start processing that
        if (dicomItem.ValueRepresentation.Code == "SQ")
        {
            var sequenceItemDataSets = dataset.GetSequence(dicomItem.Tag);
            foreach (var sequenceItemDataSet in sequenceItemDataSets)
                foreach (var sequenceItem in sequenceItemDataSet)
                    ValidateDicomItem(fi, dicomFile, sequenceItemDataSet, sequenceItem);
        }
        else
        {
            object value;
            try
            {
                value = DicomTypeTranslaterReader.GetCSharpValue(dataset, dicomItem);
            }
            catch (FormatException)
            {
                // TODO(rkm 2020-04-14) Fix this - we shouldn't just validate the "unknown value" string...
                value = $"Unknown value for {dicomItem}";
            }
            // Sometimes throws "Input string was not in a correct format"
            //var value = DicomTypeTranslaterReader.GetCSharpValue(dataset, dicomItem);

            switch (value)
            {
                case string asString:
                    Validate(fi, dicomFile, dicomItem, asString);
                    break;
                case IEnumerable<string> enumerable:
                    {
                        foreach (var s in enumerable)
                            Validate(fi, dicomFile, dicomItem, s);
                        break;
                    }
                case DateTime time when _datesAreFailures && _excludedDate != time:
                    NotifyNewFailure(FailureFrom(fi, dicomFile, time.ToString(), dicomItem.Tag.DictionaryEntry.Keyword, new[] { new FailurePart(time.ToString(), FailureClassification.Date, 0) }));
                    break;
            }
        }
    }

    private void Validate(IFileInfo fi, DicomFile dicomFile, DicomItem dicomItem, string fieldValue)
    {
        // Keyword might be "Unknown" in which case we would rather use "(xxxx,yyyy)"
        var tagName = dicomItem.Tag.DictionaryEntry.Keyword;
        //if (tagName == "Unknown" || tagName == "PrivateCreator") tagName = dicomItem.Tag.ToString(); // do this if you want PrivateCreator tags to have the numeric value preserved
        if (tagName == "Unknown") tagName = dicomItem.Tag.ToString();

        // Some strings contain null characters?!  Remove them all.
        // XXX hopefully this won't break any special character encoding (eg. UTF)
        fieldValue = fieldValue.Replace("\0", "");

        var parts = Validate(tagName, fieldValue).ToList();

        if (parts.Any())
            NotifyNewFailure(FailureFrom(fi, dicomFile, fieldValue, tagName, parts));
    }

    private void ValidateDicomPixelData(IFileInfo fi, DicomFile dicomFile, DicomDataset ds)
    {
        var modality = ds.GetValueOrDefault<string?>(DicomTag.Modality, 0, null);
        var imageType = GetImageType(ds);
        var studyInstanceUID = ds.GetValueOrDefault<string?>(DicomTag.StudyInstanceUID, 0, null);
        var seriesInstanceUID = ds.GetValueOrDefault<string?>(DicomTag.SeriesInstanceUID, 0, null);
        var sopInstanceUID = ds.GetValueOrDefault<string?>(DicomTag.SOPInstanceUID, 0, null);

        // Don't go looking for images in structured reports
        if (modality == "SR")
            return;

        Logger.Info($"Processing '{fi.FullName}'");
        try
        {
            var dicomImageObj = new DicomImage(fi.FullName);
            var numFrames = dicomImageObj.NumberOfFrames;
            for (var frameNum = 0; frameNum < numFrames; frameNum++)
            {
                Logger.Info($" Frame {frameNum} in '{fi.FullName}'");
                dicomImageObj.OverlayColor = 0xffffff; // white, as default magenta not good for tesseract
                var dicomImage = dicomImageObj.RenderImage(frameNum).AsSharpImage();
                using var memStreamOut = new System.IO.MemoryStream();
                using var mi = new MagickImage();
                using (var ms = new System.IO.MemoryStream())
                {
                    dicomImage.SaveAsBmp(ms);
                    ms.Position = 0;
                    mi.Read(ms);
                    ProcessBitmapMemStream(ms.ToArray(), fi, dicomFile, sopInstanceUID, studyInstanceUID, seriesInstanceUID, modality, imageType, 0, frameNum);
                }

                // Threshold the image to monochrome using a window size of 25 square
                // The size 25 was determined empirically based on real images (could be larger, less effective if smaller)
                mi.AdaptiveThreshold(25, 25);
                ProcessBitmapMemStream(mi, false, fi, dicomFile, sopInstanceUID, studyInstanceUID, seriesInstanceUID, modality, imageType, 0, frameNum);
                // Tesseract only works with black text on white background so run again negated
                mi.Negate();
                ProcessBitmapMemStream(mi, false, fi, dicomFile, sopInstanceUID, studyInstanceUID, seriesInstanceUID, modality, imageType, 0, frameNum);

                // Need to threshold and possibly negate the image for best results
                // Magick.NET won't read from Bitmap directly in .net core so go via MemoryStream

                //if user wants to rotate the image 90, 180 and 270 degrees
                // XXX this is done from the dicomImage, maybe need to threshold/negate here too?
                if (!_rotate) continue;

                for (var i = 0; i < 3; i++)
                {
                    //rotate image 90 degrees and run OCR again
                    using var ms = new System.IO.MemoryStream();
                    dicomImage.Mutate(x => x.Rotate(RotateMode.Rotate90));
                    dicomImage.SaveAsBmp(ms);
                    ProcessBitmapMemStream(ms.ToArray(), fi, dicomFile, sopInstanceUID, studyInstanceUID, seriesInstanceUID, modality, imageType,
                        (i + 1) * 90, frameNum);
                }
            }

            // Process all of the Overlays and all of their frames

            // Get a set of 'group' identifiers from 0x6000 to 0x60FE if group.0x0010 exists.
            // Each group will be an overlay. Note only even numbers exist, so max 16 overlays.
            var groups = new List<ushort>();
            groups.AddRange(ds.Where(x => x.Tag.Group >= 0x6000 && x.Tag.Group <= 0x60FF &&
                x.Tag.Element == 0x0010).Select(x => x.Tag.Group));

            foreach (var group in groups)
            {
                // Check NumberOfFramesInOverlay, if present
                var numframes = ds.GetValueOrDefault<ushort>(new DicomTag(group, 0x0015), 0, 0);

                // Check OverlayBitPosition, normally 0, or bit position for old-style embedded
                var bitpos = ds.GetValue<ushort>(new DicomTag(group, 0x0102), 0);

                // Load the overlay info for this group
                // See https://fo-dicom.github.io/stable/v5/api/FellowOakDicom.Imaging.DicomOverlayData.html
                DicomOverlayData overlay = new(ds, group);

                // Get overlay as black on white, best for tesseract
                var overlayBytes = overlay.Data.Data; // not GetOverlayDataS32(255, 0) which returns int[]

                // Get multiple frames?
                var numoverlayframes = overlay.NumberOfFrames;
                var overlayframesize = overlay.Rows * overlay.Columns;
                Logger.Debug($"Overlay {group - 0x6000} in '{fi.FullName}' bitpos={bitpos}, {overlay.Columns}x{overlay.Rows} x{numframes} frames, bytes={overlayBytes.Length}");

                var overlayBits = new BitArray(overlayBytes);
                // XXX can we simply multiply height by numframes to make one very long image?
                // assuming the frame data is simply concatenated in the overlaydata.
                // Not sure if this holds true for widths which are not a multiple of 8.
                for (var ovframenum = 0; ovframenum < numoverlayframes; ovframenum++)
                {
                    var overlayBuf = new byte[overlayframesize];
                    for (var ii = 0; ii < overlayframesize; ii++)
                    {
                        overlayBuf[ii] = overlayBits.Get(ovframenum * overlayframesize + ii) ? (byte)0 : (byte)255;
                    }

                    // Convert each frame into a BMP, then into a MemoryStream
                    MagickReadSettings msett = new()
                    {
                        ColorType = ColorType.Grayscale,
                        Width = overlay.Columns,
                        Height = overlay.Rows,
                        Depth = 8,
                        Format = MagickFormat.Gray
                    };
                    using var magick_image = new MagickImage(overlayBuf, msett);
                    // Write to a file, format Png or Png00 or Png8 ???
                    //magick_image.Write($"{fi.FullName}.ov{group-0x6000}.frame{ovframenum}.png", MagickFormat.Png);
                    // Tesseract only works with black text on white background so run again negated
                    magick_image.Negate();
                    ProcessBitmapMemStream(magick_image, true, fi, dicomFile, sopInstanceUID, studyInstanceUID, seriesInstanceUID, modality, imageType, 0, group, ovframenum);
                }

            }
        }
        catch (Exception e)
        {
            // An internal error should cause IsIdentifiable to exit
            Logger.Info(e, $"Could not run Tesseract on '{fi.FullName}'");
            throw new ApplicationException($"Could not run Tesseract on '{fi.FullName}'", e);

            // OR add a message to the report saying we failed to run OCR
            //string problemField = "PixelData";
            //string text = "Error running OCR on pixel data: "+e;
            //var f = factory.FailureFrom(fi, dicomFile, text, problemField, new[] { new FailurePart(text, FailureClassification.PixelText) });
            //NotifyNewFailure(f);
            // XXX do we need this?
            //_tesseractReport.FoundPixelData(fi, sopInstanceUID, pixelFormat, processedPixelFormat, studyInstanceUID, seriesInstanceUID, modality, imageType, meanConfidence, text.Length, text, rotationIfAny);
        }
    }

    private void ProcessBitmapMemStream(byte[] bytes, IFileInfo fi, DicomFile dicomFile, string sopID,
        string studyID, string seriesID, string modality, string[] imageType, int rotationIfAny = 0,
        int frame = -1, int overlay = -1)
    {
        float meanConfidence;
        string text;

        using (var page = _tesseractEngine!.Process(Pix.LoadFromMemory(bytes)))
        {
            text = page.GetText();
            text = Regex.Replace(text, @"\t|\n|\r", " ");   // XXX abrooks surely more useful to have a space?
            text = text.Trim();
            meanConfidence = page.GetMeanConfidence();
        }

        //if we find some text
        if (string.IsNullOrWhiteSpace(text)) return;

        var problemField = rotationIfAny != 0 ? $"{PixelData}{rotationIfAny}" : PixelData;

        if (text.Length < _ignoreTextLessThan)
            Logger.Debug($"Ignoring pixel data discovery in {fi.Name} of length {text.Length} because it is below the threshold {_ignoreTextLessThan}");
        else
        {
            var f = FailureFrom(fi, dicomFile, text, problemField, new[] { new FailurePart(text, FailureClassification.PixelText) });
            NotifyNewFailure(f);
            _tesseractReport?.FoundPixelData(fi, sopID, studyID, seriesID, modality, imageType, meanConfidence, text.Length, text, rotationIfAny, frame, overlay);
        }
    }

    /// <summary>
    /// Convert the provided MagickImage object to either a BMP or PGM format byte array, then process above
    /// </summary>
    /// <param name="mi"></param>
    /// <param name="forcePgm"></param>
    /// <param name="fi"></param>
    /// <param name="dicomFile"></param>
    /// <param name="sopID"></param>
    /// <param name="studyID"></param>
    /// <param name="seriesID"></param>
    /// <param name="modality"></param>
    /// <param name="imageType"></param>
    /// <param name="rotationIfAny"></param>
    /// <param name="frame"></param>
    /// <param name="overlay"></param>
    private void ProcessBitmapMemStream(MagickImage mi, bool forcePgm, IFileInfo fi, DicomFile dicomFile, string sopID, string studyID, string seriesID, string modality, string[] imageType, int rotationIfAny = 0, int frame = -1, int overlay = -1)
    {
        byte[] bytes;
        using (System.IO.MemoryStream ms = new())
        {
            if (forcePgm)
                mi.Write(ms, MagickFormat.Pgm);
            else
                mi.Write(ms);
            bytes = ms.ToArray();
        }
        ProcessBitmapMemStream(bytes, fi, dicomFile, sopID, studyID, seriesID, modality, imageType, rotationIfAny, frame, overlay);
    }

    /// <summary>
    /// Returns a 3 element array of the Dicom ImageType tag.  If there are less than 3 elements in the dataset it returns nulls.  If
    /// there are more than 3 elements it sets the final element to all remaining elements joined with backslashes 
    /// </summary>
    /// <param name="ds"></param>
    /// <returns></returns>
    private static string[] GetImageType(DicomDataset ds)
    {
        var result = new string[3];

        if (!ds.Contains(DicomTag.ImageType))
            return result;

        var values = ds.GetValues<string>(DicomTag.ImageType);

        if (values.Length > 0)
            result[0] = values[0];

        if (values.Length > 1)
            result[1] = values[1];

        if (values.Length > 2)
        {
            result[2] = "";
            for (var i = 2; i < values.Length; ++i)
                result[2] = $"{result[2]}\\{values[i]}";
        }

        return result;
    }

    private static Failure FailureFrom(IFileInfo file, DicomFile dcm, string problemValue, string problemField, IEnumerable<FailurePart> parts)
    {
        string resourcePrimaryKey;
        try
        {
            // Some DICOM files do not have SOPInstanceUID
            resourcePrimaryKey = dcm.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID);
        }
        catch (DicomDataException)
        {
            resourcePrimaryKey = "UnknownPrimaryKey";
        }
        return new Failure(parts)
        {
            Resource = file.FullName,
            ResourcePrimaryKey = resourcePrimaryKey,
            ProblemValue = problemValue,
            ProblemField = problemField
        };
    }

    protected override void DisposeImpl()
    {
        // TODO(rkm 2023-07-05)
        throw new NotImplementedException();
    }

    protected override string LogProgressNoun() => "files";
}