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


## Library Usage 

To use the nuget package create a new project and add a reference to the package:

```
dotnet new console -n MyExample
cd MyExample
dotnet add package IsIdentifiable
```

Open Program.cs and enter the following:

```csharp
using IsIdentifiable.Options;
using IsIdentifiable.Reporting;
using IsIdentifiable.Reporting.Reports;
using IsIdentifiable.Runners;
using System;

// Where to put the output, in this case just to memory
var dest = new ToMemoryFailureReport();

// Your runner that fetches and validates data
var runner = new CustomRunner(dest);

// fetch and analyise data
runner.Run();

Console.WriteLine("Failures:" + dest.Failures.Count);
Console.WriteLine("Bad Parts:" + dest.Failures[0].Parts.Count);
Console.WriteLine("Bad Part 0:" + dest.Failures[0].Parts[0].Word);
Console.WriteLine("Bad Part 1:" + dest.Failures[1].Parts[1].Word);

class CustomRunner : IsIdentifiableAbstractRunner
{
    public CustomRunner(IFailureReport report) :base(new IsIdentifiableBaseOptions(),report)
    {
    }

    public override int Run()
    {
        // validate some example data we might have fetched
        var badParts = Validate("SomeText", "Patient DoB is 2Mar he is my best buddy. CHI number is 0101010101");

        // You can ignore or adjust these if you want before passing to destination reports

        // Pass all parts as a Failure to the destination reports
        AddToReports(new Failure(badParts));

        // Record progress
        DoneRows(1);

        // Once all data is finished being fetched, close the destination reports
        CloseReports();

        return 0;
    }
}
```


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

