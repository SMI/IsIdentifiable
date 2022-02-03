using System.Collections.Generic;
using FAnsi.Discovery;

namespace IsIdentifiable.Allowlists
{
    public class DiscoveredColumnAllowlist : IAllowlistSource
    {
        private readonly DiscoveredTable _discoveredTable;
        private DiscoveredColumn _column;

        public DiscoveredColumnAllowlist(DiscoveredColumn col)
        {
            _discoveredTable = col.Table;
            _column = col;
        }

        public IEnumerable<string> GetAllowlist()
        {
            var colName = _column.GetRuntimeName();

            using (var con = _discoveredTable.Database.Server.GetConnection())
            {
                con.Open();

                var cmd = _discoveredTable.GetCommand("Select DISTINCT " + _column.GetFullyQualifiedName() + " FROM " + _discoveredTable.GetFullyQualifiedName(), con);
                var r = cmd.ExecuteReader();

                while(r.Read())
                {
                    var o = r[colName] as string;

                    if(o != null)
                        yield return o.Trim();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}