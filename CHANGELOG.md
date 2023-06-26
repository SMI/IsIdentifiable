# Changelog

# [Unreleased]

...

# [0.1.0] - 2023-04-12

## Added

-   Add CodeQL workflow for GitHub code scanning #240 and #242
-   Add coverage #267
-   Add filesystem abstractions #290
-   Add ability to create `CsvDestination` with an existing `CsvConfiguration` #291

## Fixed

-   Explicitly pass `top` view to `Application.Run` #294
-   Cleanup warnings and CodeQL issues #292
-   Validate the date part of each possible CHI before complaining #247

# [0.0.9] - 2022-11-21

## Added

-   Bump Microsoft.Extensions.Caching.Memory from 6.0.1 to 7.0.0
-   Bump Microsoft.NET.Test.Sdk from 17.3.2 to 17.4.0
-   Bump Microsoft.Extensions.FileSystemGlobbing from 6.0.0 to 7.0.0
-   Bump CsvHelper from 30.0.0 to 30.0.1
-   Bump HIC.RDMP.Plugin from 8.0.5 to 8.0.6
-   Bump Magick.NET-Q16-AnyCPU from 12.2.0 to 12.2.1
-   Bump System.IO.Abstractions from 17.2.3 to 17.2.26
-   Bump NUnit3TestAdapter from 4.3.0 to 4.3.1
-   Fix dependabot config tagging Thomas

# [0.0.8] - 2022-11-04

## Added

-   Support for running on 'non dicom' MongoDb databases. This is now the default. Pass `--isdicomfiles` if your MongoDb contains serialized dicom files.
-   New flag `--top` to only run on a subset of the data available (e.g. `top 1000`). Currently only supported by relational database and csv runners
-   Added ability to ignore whole columns in reviewer by pressing `Del` on the column and confirming
-   Support for naming servers in `Targets.yaml` for main `ii` binary instead of connection strings (e.g. `-d myserver` for running on relational dbs)
-   You can now pass a directory name to the `file` verb to process all csv files in that directory.
-   Added `-g` option to the `file` verb to process multiple csv files e.g. `**/*.csv`. This option is only valid when specifying a directory for `-f`

## Fixed

-   IsIdentifiable reviewer no longer complains when `Targets.yaml` is missing

# [0.0.7] - 2022-08-24

## Added

-   Made `--rulesfile` CLI argument default to `Rules.yaml`
-   Added TRACE progress logging to `ii` CLI tool
-   Stanford NLP daemon now runs self contained
-   Bump HIC.FAnsiSql from 2.0.4 to 2.0.5

## Fixed

-   Fixed missing dlls for running Tesseract OCR on linux

# [0.0.6] - 2022-08-17

## Added

-   Added new command line flag `-y somefile.yaml` in `ii` CLI tool to specify a custom config file
-   Progress is now logged to Trace and enabled by default in `ii`. Library users can enable this feature by setting `LogProgressEvery` (defaults to null)

## Changed

-   `ii` startup errors are written to stderr instead of stdout

# [0.0.5] - 2022-07-20

## Dependencies

-   New dependency Equ 2.3.0
-   New dependency fo-dicom.Imaging.ImageSharp 5.0.3
-   Bump CommandLineParser from 2.8.0 to 2.9.1
-   Bump CsvHelper from 27.2.1 to 28.0.1
-   Bump HIC.DicomTypeTranslation from 3.0.0 to 4.0.1
-   Bump HIC.FAnsiSql from 2.0.3 to 2.0.4
-   Bump HIC.RDMP.Plugin from 7.0.7 to 7.0.14
-   Bump MSTest.TestAdapter from 2.2.8 to 2.2.10
-   Bump MSTest.TestFramework from 2.2.8 to 2.2.10
-   Bump Magick.NET-Q16-AnyCPU from 10.0.0 to 11.3.0
-   Bump Microsoft.NET.Test.Sdk from 17.1.0 to 17.2.0
-   Bump Moq from 4.17.1 to 4.18.1
-   Bump NLog from 4.7.14 to 5.0.1
-   Bump NUnit from 3.13.2 to 3.13.3
-   Bump System.IO.Abstractions from 16.1.15 to 17.0.23
-   Bump Terminal.Gui from 1.4.0 to 1.6.4
-   Removed dependency fo-dicom.Drawing 4.0.8

# [0.0.4] - 2022-03-03

-   Added IsIdentifiable RDMP plugin

# [0.0.3] - 2022-03-01

-   Added `UpdateStrategy.RedactionWord` to customise the substitution value for PII when updating the database
-   Moved redaction code to `IsIdentifiable.Redacting` namespace
-   Retargetted at dotnet standard 2.1
-   Removed dependency on Terminal.Gui from library (still part of the ii CLI)

# [0.0.2] - 2022-02-10

-   Made it easier to subclass `IsIdentifiableAbstractRunner` and add custom reports

# [0.0.1] - 2022-02-07

Initial version

[Unreleased]: https://github.com/SMI/IsIdentifiable/compare/v0.1.0..main
[0.1.0]: https://github.com/SMI/IsIdentifiable/compare/v0.0.9..v0.1.0
[0.0.9]: https://github.com/SMI/IsIdentifiable/compare/v0.0.8..v0.0.9
[0.0.8]: https://github.com/SMI/IsIdentifiable/compare/v0.0.7..v0.0.8
[0.0.7]: https://github.com/SMI/IsIdentifiable/compare/v0.0.6..v0.0.7
[0.0.6]: https://github.com/SMI/IsIdentifiable/compare/v0.0.5..v0.0.6
[0.0.5]: https://github.com/SMI/IsIdentifiable/compare/v0.0.4..v0.0.5
[0.0.4]: https://github.com/SMI/IsIdentifiable/compare/v0.0.3..v0.0.4
[0.0.3]: https://github.com/SMI/IsIdentifiable/compare/v0.0.2..v0.0.3
[0.0.2]: https://github.com/SMI/IsIdentifiable/releases/tag/v0.0.2
[0.0.1]: https://github.com/SMI/IsIdentifiable/releases/tag/v0.0.1
