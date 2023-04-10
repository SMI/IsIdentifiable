using System.Collections.Generic;
using FAnsi.Discovery;

namespace IsIdentifiable.Whitelists;

/// <summary>
/// Generates a list of allowed (ignored) strings based on the contents of a
/// table in a remote database.  All values in the referenced column will
/// be ignored as false positives when detected by NLP or other reporting rules
/// </summary>
public class DiscoveredColumnAllowlist : IAllowlistSource
{
    private readonly DiscoveredTable _discoveredTable;
    private DiscoveredColumn _column;

    /// <summary>
    /// Creates a new instance prepared to fetch all distinct values in the
    /// given <paramref name="col"/> as an allowed values list.
    /// </summary>
    /// <param name="col"></param>
    public DiscoveredColumnAllowlist(DiscoveredColumn col)
    {
        _discoveredTable = col.Table;
        _column = col;
    }

    /// <summary>
    /// Connects to the database and reads all distinct values.  Returns the list of
    /// values so they can be used for ignoring other system rules (e.g. NLP false positives)
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllowlist()
    {
        var colName = _column.GetRuntimeName();

        using var con = _discoveredTable.Database.Server.GetConnection();
        con.Open();

        var cmd = _discoveredTable.GetCommand(
            $"Select DISTINCT {_column.GetFullyQualifiedName()} FROM {_discoveredTable.GetFullyQualifiedName()}", con);
        var r = cmd.ExecuteReader();

        while(r.Read())
        {
            if(r[colName] is string o)
                yield return o.Trim();
        }
    }

    /// <summary>
    /// Overridden to do nothing
    /// </summary>
    public void Dispose()
    {
    }
}