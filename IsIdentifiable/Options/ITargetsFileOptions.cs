namespace IsIdentifiable.Options;

/// <summary>
/// Options shared interface for options that care about named relational databases (i.e. Targets.yaml)
/// </summary>
public interface ITargetsFileOptions
{
    /// <summary>
    /// Location of database connection strings file
    /// </summary>
    string TargetsFile { get; set; }

}
