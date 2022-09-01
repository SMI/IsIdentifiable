using CommandLine;
using FAnsi;


namespace IsIdentifiable.Options;

/// <summary>
/// Base class for all options that go to a datasource.  Implement on each new place e.g. dicom, DBMS, CSV etc
/// </summary>
public class IsIdentifiableBaseOptions
{
    /// <summary>
    /// Optional. Full connection string to the database storing the Allowlist of valid entries
    /// </summary>
    [Option(HelpText = "Optional. Full connection string to the database storing the Allowlist of valid entries")]
    public string AllowlistConnectionString { get; set; }

    /// <summary>
    /// Optional. The DBMS provider of the Allowlist table e.g. MySql
    /// </summary>
    [Option(HelpText = "Optional. The DBMS provider of the Allowlist table e.g. MySql")]
    public DatabaseType? AllowlistDatabaseType { get; set; }

    /// <summary>
    /// Optional. The unqualified name of the Allowlist table
    /// </summary>
    [Option(HelpText = "Optional. The unqualified name of the Allowlist table")]
    public string AllowlistTableName { get; set; }

    /// <summary>
    /// Optional. The column in AllowlistTableName which contains the Allowlist elements
    /// </summary>
    [Option(HelpText = "Optional. The column in AllowlistTableName which contains the Allowlist elements")]
    public string AllowlistColumn { get; set; }

    /// <summary>
    /// Optional. Path to a CSV file containing a single untitled column of Allowlist values
    /// </summary>
    [Option(HelpText = "Optional. Path to a CSV file containing a single untitled column of Allowlist values")]
    public string AllowlistCsv { get; set; }

    /// <summary>
    /// Optional. Generate a report on the proportion of values failing validation (for each column)
    /// </summary>
    [Option(HelpText = "Optional. Generate a report on the proportion of values failing validation (for each column)")]
    public bool ColumnReport { get; set; }

    /// <summary>
    /// Optional. Generate a report listing every unique value failing validation (and the column the value failed in)
    /// </summary>
    [Option(HelpText = "Optional. Generate a report listing every unique value failing validation (and the column the value failed in)")]
    public bool ValuesReport { get; set; }

    /// <summary>
    /// Optional. Generate a full failure storage report that persists Failure objects in a manner that they can be retrieved.
    /// </summary>
    [Option(HelpText = "Optional. Generate a full failure storage report that persists Failure objects in a manner that they can be retrieved.")]
    public bool StoreReport { get; set; }

    /// <summary>
    /// Optional - If specified reports will be generated in the given folder.  If not specified, current directory is used (unless an alternate destination option is picked)
    /// </summary>
    [Option(HelpText = "Optional - If specified reports will be generated in the given folder.  If not specified, current directory is used (unless an alternate destination option is picked)")]
    public string DestinationCsvFolder { get; set; }

    /// <summary>
    /// Optional - If specified, the given separator will be used instead of ,.  Includes support for \t for tab and \r\n.
    /// </summary>
    [Option(HelpText = @"Optional - If specified, the given separator will be used instead of ,.  Includes support for \t for tab and \r\n.")]
    public string DestinationCsvSeparator { get; set; }

    /// <summary>
    /// Optional - If specified all tabs, newlines (\r and \n) and 2+ spaces will be stripped from the values written as output (applies to all output formats)
    /// </summary>
    [Option(HelpText = @"Optional - If specified all tabs, newlines (\r and \n) and 2+ spaces will be stripped from the values written as output (applies to all output formats)")]
    public bool DestinationNoWhitespace { get; set; }

    /// <summary>
    /// Optional. Full connection string to the database in which to store the report results
    /// </summary>
    [Option(HelpText = "Optional. Full connection string to the database in which to store the report results")]
    public string DestinationConnectionString { get; set; }

    /// <summary>
    /// Optional. The DBMS provider of DestinationConnectionString e.g. MySql
    /// </summary>
    [Option(HelpText = "Optional. The DBMS provider of DestinationConnectionString e.g. MySql")]
    public DatabaseType? DestinationDatabaseType { get; set; }

    /// <summary>
    /// Optional. If specified postcodes will not be reported as failures
    /// </summary>
    [Option(HelpText = "Optional. If specified postcodes will not be reported as failures")]
    public bool IgnorePostcodes { get; set; }

    /// <summary>
    /// Optional. Comma separated list of columns/tags which should be ignored and not processed
    /// </summary>
    [Option(HelpText = "Optional. Comma separated list of columns/tags which should be ignored and not processed")]
    public string SkipColumns { get; set; }

    /// <summary>
    /// Optional. If set and using a 7 class NER model then DATE and TIME objects will not be considered failures.
    /// </summary>
    [Option(HelpText = "Optional. If set and using a 7 class NER model then DATE and TIME objects will not be considered failures.")]
    public bool IgnoreDatesInText { get; set; }

    /// <summary>
    /// Optional. Set to control the max size of the in-memory store of processed before the get written out to any destinations. Only
    /// makes sense for reports that don't perform any aggregation across the data.  Defaults to <see cref="MaxCacheSizeDefault"/>
    /// </summary>
    [Option(HelpText = "Optional. Set to control the max size of the in-memory store of processed before the get written out to any destinations. Only makes sense for reports that don't perform any aggregation across the data", Default = MaxCacheSizeDefault)]
    public int? MaxCacheSize { get; set; } = MaxCacheSizeDefault;

    /// <summary>
    /// Default value for <see cref="MaxCacheSize"/>
    /// </summary>
    public const int MaxCacheSizeDefault = 10000;

    /// <summary>
    /// Default value for <see cref="RulesFile"/>.
    /// </summary>
    public const string DefaultRulesFile = "Rules.yaml";

    /// <summary>
    /// Optional. Filename of additional rules in yaml format.  See also <see cref="RulesDirectory"/> to use multiple
    /// seperate yaml files for rules
    /// </summary>
    [Option(HelpText = "Optional. Filename of additional rules in yaml format.", Default = DefaultRulesFile)]
    public string RulesFile { get; set; } = DefaultRulesFile;

    /// <summary>
    /// Optional. Directory of additional rules in yaml format.
    /// </summary>

    [Option(HelpText = "Optional. Directory of additional rules in yaml format.")]
    public string RulesDirectory { get; set; }

    /// <summary>
    /// Optional.  Maximum number of answers to cache per column.  Defaults to <see cref="MaxValidationCacheSizeDefault"/>
    /// </summary>

    [Option(HelpText = "Optional.  Maximum number of answers to cache per column.", Default = MaxValidationCacheSizeDefault)]
    public int? MaxValidationCacheSize { get; set; } = MaxValidationCacheSizeDefault;

    /// <summary>
    /// Optional.  Set to a stop processing after x records e.g. only evalute top 1000 records of a table/file.  Currently only supported for csv/database
    /// </summary>
    [Option(HelpText = "Optional.  Set to stop processing after x records e.g. only evalute top 1000 records of a table/file.  Currently only supported for csv/database", Default = -1)]
    public int Top { get; set; } = -1;

    /// <summary>
    /// Default for <see cref="MaxValidationCacheSize"/>
    /// </summary>
    public const int MaxValidationCacheSizeDefault = 1_000_000;


    /// <summary>
    /// Default for <see cref="TargetName"/>
    /// </summary>
    public const string TargetNameDefault = "Unknown";

    /// <summary>
    /// <para>
    /// The value returned by <see cref="GetTargetName"/> that describes where these options
    /// point.  Will be used to describe report outputs in a manner meaningful to the user.
    /// For example with the name of the table that was read for data.  Defaults to "Unknown".
    /// </para>
    /// 
    /// <para>Only applies to <see cref="IsIdentifiableBaseOptions"/>.  Other options may override
    /// <see cref="GetTargetName"/> and so not respect this property.</para>
    /// </summary>
    public string TargetName { get; set; } = TargetNameDefault;


    /// <summary>
    /// Returns a short string with no spaces or punctuation that describes the target.  This will be used
    /// for naming output reports e.g. "biochemistry" , "mydir" etc
    /// </summary>
    /// <returns></returns>
    public virtual string GetTargetName()
    {
        return TargetName;
    }

    /// <summary>
    /// Throw exceptions if the selected options are incompatible
    /// </summary>
    public virtual void ValidateOptions()
    {

    }

    /// <summary>
    /// Populates class options that have not been specified on the command line directly by using the values (if any) in the
    /// <paramref name="globalOpts"/>
    /// </summary>
    /// <param name="globalOpts"></param>
    public virtual void InheritValuesFrom(IsIdentifiableBaseOptions globalOpts)
    {
        if (string.IsNullOrWhiteSpace(AllowlistConnectionString))
            AllowlistConnectionString = globalOpts.AllowlistConnectionString;

        if (AllowlistDatabaseType == null && globalOpts.AllowlistDatabaseType.HasValue)
            AllowlistDatabaseType = globalOpts.AllowlistDatabaseType.Value;

        if(TargetName == TargetNameDefault && globalOpts.TargetName != null && globalOpts.TargetName != TargetNameDefault)
            TargetName = globalOpts.TargetName;

        if (string.IsNullOrWhiteSpace(AllowlistTableName))
            AllowlistTableName = globalOpts.AllowlistTableName;

        if (string.IsNullOrWhiteSpace(AllowlistColumn))
            AllowlistColumn = globalOpts.AllowlistColumn;

        if (string.IsNullOrWhiteSpace(AllowlistCsv))
            AllowlistCsv = globalOpts.AllowlistCsv;

        if (globalOpts.ColumnReport)
            ColumnReport = true;

        if (globalOpts.ValuesReport)
            ValuesReport = true;

        if (globalOpts.StoreReport)
            StoreReport = true;

        if (string.IsNullOrWhiteSpace(DestinationCsvFolder))
            DestinationCsvFolder = globalOpts.DestinationCsvFolder;

        if (string.IsNullOrWhiteSpace(DestinationCsvSeparator))
            DestinationCsvSeparator = globalOpts.DestinationCsvSeparator;

        if (globalOpts.DestinationNoWhitespace)
            DestinationNoWhitespace = true;

        if (string.IsNullOrWhiteSpace(DestinationConnectionString))
            DestinationConnectionString = globalOpts.DestinationConnectionString;

        if (DestinationDatabaseType == null && globalOpts.DestinationDatabaseType.HasValue)
            DestinationDatabaseType = globalOpts.DestinationDatabaseType.Value;

        if (globalOpts.IgnorePostcodes)
            IgnorePostcodes = true;

        if (string.IsNullOrWhiteSpace(SkipColumns))
            SkipColumns = globalOpts.SkipColumns;

        if (globalOpts.IgnoreDatesInText)
            IgnoreDatesInText = true;

        if (MaxCacheSize == MaxCacheSizeDefault && globalOpts.MaxCacheSize.HasValue)
            MaxCacheSize = globalOpts.MaxCacheSize.Value;

        // if globals specifies a RulesFile and our instance has the default or empty
        if (!string.IsNullOrWhiteSpace(globalOpts.RulesFile) && (string.IsNullOrWhiteSpace(RulesFile) || RulesFile == DefaultRulesFile))
            RulesFile = globalOpts.RulesFile;

        if (string.IsNullOrWhiteSpace(RulesDirectory))
            RulesDirectory = globalOpts.RulesDirectory;

        if (MaxValidationCacheSize == MaxValidationCacheSizeDefault && globalOpts.MaxValidationCacheSize.HasValue)
            MaxValidationCacheSize = globalOpts.MaxValidationCacheSize.Value;

        // if global options specifies to only run on x records and we don't have an explicit declaration for that property
        if (Top <= 0 && globalOpts.Top > 0)
            Top = globalOpts.Top;
    }
}