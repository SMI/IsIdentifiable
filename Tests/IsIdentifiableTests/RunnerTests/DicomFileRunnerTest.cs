using FellowOakDicom;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting.Reports;
using IsIdentifiable.Runners;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace IsIdentifiable.Tests.RunnerTests;

public class DicomFileRunnerTest
{
    #region Fixture Methods

    private const string DataDirectory = @"../../../../../data/";
    private DirectoryInfo _tessDir;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var testRulesDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "IsIdentifiableRules"));
        testRulesDir.Create();

        _tessDir = new DirectoryInfo(Path.Combine(testRulesDir.Parent.FullName, "tessdata"));
        _tessDir.Create();
        var dest = Path.Combine(_tessDir.FullName, "eng.traineddata");
        if (!File.Exists(dest))
            File.Copy(Path.Combine(DataDirectory, "tessdata", "eng.traineddata"), dest);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() { }

    #endregion

    #region Test Methods

    [SetUp]
    public void SetUp() { }

    [TearDown]
    public void TearDown() { }

    #endregion

    #region Tests

    [TestCase(true)]
    [TestCase(false)]
    public void IgnorePixelDataLessThan(bool ignoreShortText)
    {
        var opts = new IsIdentifiableDicomFileOptions
        {
            ColumnReport = true,
            TessDirectory = _tessDir.FullName,

            // If we ignore less than 170 then only 1 bit of text should be reported.
            // NOTE(rkm 2020-11-16) The test image should report 3 bits of text with lengths 123, 127, and 170.
            IgnoreTextLessThan = ignoreShortText ? 170 : 0U
        };

        var fileSystem = new System.IO.Abstractions.FileSystem();

        var fileName = Path.Combine(TestContext.CurrentContext.TestDirectory, nameof(DicomFileRunnerTest), "f1.dcm");
        TestData.Create(fileSystem.FileInfo.New(fileName), TestData.BURNED_IN_TEXT_IMG);

        var runner = new DicomFileRunner(opts, fileSystem);

        var fileInfo = fileSystem.FileInfo.New(fileName);
        Assert.That(fileInfo.Exists, Is.True);

        var toMemory = new ToMemoryFailureReport();
        runner.Reports.Add(toMemory);

        runner.ValidateDicomFile(fileInfo);

        var failures = toMemory.Failures.ToList();
        Assert.That(failures, Has.Count.EqualTo(ignoreShortText ? 1 : 3));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SkipPixelSafeTags(bool skipSafePixelValidation)
    {
        // Arrange

        var opts = new IsIdentifiableDicomFileOptions
        {
            ColumnReport = true,
            TessDirectory = _tessDir.FullName,
            SkipSafePixelValidation = skipSafePixelValidation,
        };

        var fileSystem = new System.IO.Abstractions.FileSystem();

        var fileName = Path.Combine(TestContext.CurrentContext.TestDirectory, nameof(DicomFileRunnerTest), "f1.dcm");
        TestData.Create(fileSystem.FileInfo.New(fileName), TestData.IMG_013);

        var runner = new DicomFileRunner(opts, fileSystem);

        var fileInfo = fileSystem.FileInfo.New(fileName);
        Assert.That(fileInfo.Exists, Is.True);

        // Act

        runner.ValidateDicomFile(fileInfo);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(runner.FilesValidated, Is.EqualTo(1));
            Assert.That(runner.PixelFilesValidated, Is.EqualTo(skipSafePixelValidation ? 0 : 1));
        });
    }

    [Test]
    public void SkipPixelSR()
    {
        // Arrange

        var opts = new IsIdentifiableDicomFileOptions
        {
            ColumnReport = true,
            TessDirectory = _tessDir.FullName,
            SkipSafePixelValidation = false,
        };

        var fileName = Path.Combine(TestContext.CurrentContext.TestDirectory, nameof(DicomFileRunnerTest), "SR.dcm");
        var ds = new DicomDataset()
        {
            {DicomTag.Modality, "SR" },
            {DicomTag.SOPClassUID, DicomUID.BasicTextSRStorage },
            {DicomTag.SOPInstanceUID, "1" },
        };
        var df = new DicomFile(ds);
        df.Save(fileName);

        var fileSystem = new System.IO.Abstractions.FileSystem();
        var fileInfo = fileSystem.FileInfo.New(fileName);
        Assert.That(fileInfo.Exists, Is.True);

        var runner = new DicomFileRunner(opts, fileSystem);

        // Act

        runner.ValidateDicomFile(fileInfo);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(runner.FilesValidated, Is.EqualTo(1));
            Assert.That(runner.PixelFilesValidated, Is.EqualTo(0));
        });
    }

    #endregion
}
