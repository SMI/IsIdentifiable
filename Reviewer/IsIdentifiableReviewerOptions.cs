using CommandLine;
using IsIdentifiableReviewer.Out;
using System.IO;

namespace IsIdentifiableReviewer
{
    /// <summary>
    /// CLI options for the reviewer
    /// </summary>
    public class IsIdentifiableReviewerOptions
    {
        private const string DefaultTargets = "Targets.yaml";

        [Option('f', "file",
            Required = false,
            HelpText = "[Optional] Pre load an existing failures file"
        )]
        public string FailuresCsv { get; set; }

        [Option('u', "unattended",
            Required = false,
            HelpText = "[Optional] Runs the application automatically processing existing update/ignore rules.  Failures not matching either are written to a new file with this path"
        )]
        public string UnattendedOutputPath { get; set; }

        [Option('t', "targets",
            Required = false,
            Default = DefaultTargets,
            HelpText = "Location of database connection strings file (for issuing UPDATE statements)"
        )]
        public string TargetsFile { get; set; } = DefaultTargets;

        [Option('i', "ignore",
            Required = false,
            Default = IgnoreRuleGenerator.DefaultFileName,
            HelpText = "File containing rules for ignoring validation errors"
        )]
        public string IgnoreList { get; set; } = IgnoreRuleGenerator.DefaultFileName;

        [Option('r', "redlist",
            Required = false,
            Default = RowUpdater.DefaultFileName,
            HelpText = "File containing rules for when to issue UPDATE statements"
        )]
        public string RedList { get; set; } = RowUpdater.DefaultFileName;


        [Option('o', "only-rules",
            Required = false,
            Default = false,
            HelpText = "Specify to make GUI UPDATE choices only create new rules instead of going to database"
        )]
        public bool OnlyRules { get; set; }

        /// <summary>
        /// Sets UseSystemConsole to true for Terminal.gui (i.e. uses the NetDriver which is based on System.Console)
        /// </summary>
        [Option("usc",HelpText = "Sets UseSystemConsole to true for Terminal.gui (i.e. uses the NetDriver which is based on System.Console)")]
        public bool UseSystemConsole { get; internal set; }

        /// <summary>
        /// Sets the user interface to use a specific color palette yaml file
        /// </summary>
        [Option("theme", HelpText = "Sets the user interface to use a specific color palette yaml file")]
        public FileInfo Theme { get; set; }
    }
}
