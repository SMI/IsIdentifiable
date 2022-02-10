[![.NET Core](https://github.com/SMI/IsIdentifiable/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/SMI/IsIdentifiable/actions/workflows/dotnet-core.yml) [![Total alerts](https://img.shields.io/lgtm/alerts/g/SMI/IsIdentifiable.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/SMI/IsIdentifiable/alerts/) [![NuGet Badge](https://buildstats.info/nuget/IsIdentifiable)](https://www.nuget.org/packages/IsIdentifiable/)


# IsIdentifiable
A tool for detecting identifiable information in data sources.  Out of the box supports:

- CSV
- DICOM
- Relational Database Tables (Sql Server, MySql, Postgres, Oracle)

![Demo Video](/isidentifiable.gif)

Rules base is driven by regular expressions and plugin services (e.g. Natural Language Processing).  Also includes a reviewer/redactor tool for processing false positives and updating the rules base.

- [Detector Documentation](./IsIdentifiable/README.md)
- [Reviewer Documentation](./Reviewer/README.md)

There is a [standalone command line tool called ii](./ii/README.md) for running directly or you can use the [nuget package](https://www.nuget.org/packages/IsIdentifiable/) in your own code to evaluate data.

## Building

To build and run tests you must first download the [NLP english data file for Tesseract](https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata) to the `data\tessdata\` directory.  Or use the following script:

```
 cd .\data\tessdata\
 
 # Windows
 ./download.ps1

 # Linux
 . ./download.sh
```

Then build and run (from the root of the repository)
```
dotnet build
dotnet test
```

