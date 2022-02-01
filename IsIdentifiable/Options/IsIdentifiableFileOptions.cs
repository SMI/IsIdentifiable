using System.Globalization;
using System.IO;
using CommandLine;

namespace IsIdentifiable.Options
{
    [Verb("file", HelpText = "Run tool on delimited textual data file e.g. csv")]
    public class IsIdentifiableFileOptions : IsIdentifiableBaseOptions
    {
        
        [Option('f', HelpText = "Path to a file to be evaluated", Required = true)]
        public FileInfo File { get; set; }

        [Option('c', HelpText = "The culture of dates, numbers etc if different from system culture")]
        public string Culture {get;set;}

        public override string GetTargetName()
        {
            return File.Name;
        }
    }
}
