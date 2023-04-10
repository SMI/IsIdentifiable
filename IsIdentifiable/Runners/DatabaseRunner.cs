﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO.Abstractions;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using IsIdentifiable.Failures;
using IsIdentifiable.Options;
using IsIdentifiable.Reporting;
using NLog;

namespace IsIdentifiable.Runners;

/// <summary>
/// IsIdentifiable runner which pulls data from a relational database 
/// table and evaluates it for identifiable information
/// </summary>
public class DatabaseRunner : IsIdentifiableAbstractRunner
{
    private readonly IsIdentifiableRelationalDatabaseOptions _opts;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private DiscoveredColumn[] _columns;
    private string[] _columnsNames;
    private bool[] _stringColumns;
    private DatabaseFailureFactory _factory;

    /// <summary>
    /// Creates a new instance that will read from the 
    /// listed <see cref="IsIdentifiableRelationalDatabaseOptions.DatabaseConnectionString"/>
    /// in <paramref name="opts"/>
    /// </summary>
    /// <param name="opts">Database to read from and rules settings</param>
    /// <param name="fileSystem"></param>
    public DatabaseRunner(IsIdentifiableRelationalDatabaseOptions opts, IFileSystem fileSystem)
        : base(opts, fileSystem)
    {
        _opts = opts;
        LogProgressNoun = "records";
    }

    /// <summary>
    /// Connects to the database server and fetches data from the remote table.  All
    /// records fetched are evaluated for identifiable data
    /// </summary>
    /// <returns>0 if all went well</returns>
    public override int Run()
    {
        DiscoveredTable tbl = GetServer(_opts.DatabaseConnectionString, _opts.DatabaseType, _opts.TableName);
        var server = tbl.Database.Server;

        _factory = new DatabaseFailureFactory(tbl);

        _columns = tbl.DiscoverColumns();
        _columnsNames = _columns.Select(c => c.GetRuntimeName()).ToArray();
        _stringColumns = _columns.Select(c => c.GetGuesser().Guess.CSharpType == typeof(string)).ToArray();

        using (var con = server.GetConnection())
        {
            con.Open();

            TopXResponse top = _opts.Top > 0 ? server.GetQuerySyntaxHelper().HowDoWeAchieveTopX(_opts.Top) : null;

            // assembles command 'SELECT TOP x a,b,c from Tbl'
            // or for MySql/Oracle 'SELECT a,b,c from Tbl LIMIT x'

            var cmd = server.GetCommand(
$@"SELECT 
{(top is { Location: QueryComponent.SELECT } ? top.SQL : "")}
{string.Join($",{Environment.NewLine}", _columns.Select(c => c.GetFullyQualifiedName()))}
FROM 
{tbl.GetFullyQualifiedName()}
{(top is { Location: QueryComponent.Postfix } ? top.SQL : "")}"
            ,con);

            _logger.Info($"About to send command:{Environment.NewLine}{cmd.CommandText}");

            using var reader = cmd.ExecuteReader();

            foreach (var failure in reader.Cast<DbDataRecord>().SelectMany(GetFailuresIfAny)) AddToReports(failure);

            CloseReports();
        }
        return 0;
    }

    private IEnumerable<Reporting.Failure> GetFailuresIfAny(DbDataRecord record)
    {
        //Get the primary key of the current row
        var primaryKey = _factory.PrimaryKeys.Select(k => record[k.GetRuntimeName()].ToString()).ToArray();

        //For each column in the table
        for (var i = 0; i < _columnsNames.Length; i++)
            //If it is a string column
            if (_stringColumns[i])
            {
                var asString = record[i] as string;

                if (string.IsNullOrWhiteSpace(asString))
                    continue;

                // Some strings contain null characters?!  Remove them all.
                // XXX hopefully this won't break any special character encoding (eg. UTF)
                var parts = asString.Split('\\').SelectMany(part => Validate(_columnsNames[i], part.Replace("\0", ""))).ToList();
                if (parts.Any())
                    yield return _factory.Create(_columnsNames[i], asString, parts, primaryKey);
            }

        DoneRows(1);
    }
}