using CommandLine;
using IsIdentifiableReviewer.Out;
using System;
using System.IO;

namespace IsIdentifiableReviewer
{
    /// <summary>
    /// CLI options for the reviewer
    /// </summary>
    [Verb("review",HelpText = "Review or redact the StoreReport output of an IsIdentifiable run")]
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
        public string Theme { get; set; }


        /// <summary>
        /// Populates values in this instance where no value yet exists and there is a value in <paramref name="globalOpts"/>
        /// to inherit.
        /// </summary>
        /// <param name="globalOpts"></param>
        public virtual void InheritValuesFrom(IsIdentifiableReviewerOptions globalOpts)
        {
            if(globalOpts == null)
                throw new ArgumentNullException(nameof(globalOpts));

            // if we don't have a value for it yet
            if (string.IsNullOrWhiteSpace(TargetsFile) || TargetsFile == DefaultTargets)
                // and global configs has got a value for it
                if (!string.IsNullOrWhiteSpace(globalOpts.TargetsFile))
                    TargetsFile = globalOpts.TargetsFile; // use the globals config value

            if (string.IsNullOrWhiteSpace(IgnoreList) || IgnoreList == IgnoreRuleGenerator.DefaultFileName)
                if (!string.IsNullOrWhiteSpace(globalOpts.IgnoreList))
                    IgnoreList = globalOpts.IgnoreList;

            if (string.IsNullOrWhiteSpace(RedList) || RedList == RowUpdater.DefaultFileName)
                if (!string.IsNullOrWhiteSpace(globalOpts.RedList))
                    RedList = globalOpts.RedList;

            if (Theme == null && !string.IsNullOrWhiteSpace(globalOpts.Theme))
                Theme = globalOpts.Theme;
        }
    }
}
