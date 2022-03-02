using IsIdentifiable.Options;
using IsIdentifiable.Reporting;
using IsIdentifiable.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using ReusableLibraryCode.Progress;
using System.Data;
using YamlDotNet.Serialization;

namespace IsIdentifiablePlugin;

/// <summary>
/// Pipeline component that validates data that is flowing through an RDMP
/// pipeline for PII (personally identifiable information)
/// </summary>
public class IsIdentifiablePipelineComponent : IDataFlowComponent<DataTable>
{
    private CustomRunner _runner;

    [DemandsInitialization("YAML file with the IsIdentifiable rules (regex, NLP, report formats etc)",Mandatory = true)]
    public string YamlConfigFile { get; set; }

    public void Abort(IDataLoadEventListener listener)
    {
        
    }

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
        _runner?.Dispose();
    }

    public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if(_runner == null)
        {
            var deserializer = new Deserializer();
            var opts = deserializer.Deserialize<IsIdentifiableBaseOptions>(File.ReadAllText(YamlConfigFile));
            _runner = new CustomRunner(opts);
        }

        _runner.Run(toProcess);

        return toProcess;
    }
}

class CustomRunner : IsIdentifiableAbstractRunner
{
    public CustomRunner(IsIdentifiableBaseOptions options) : base(options)
    {
    }
    public void Run(DataTable dt)
    {
        foreach (DataRow row in dt.Rows)
        {
            foreach (DataColumn col in dt.Columns)
            {
                // validate some example data we might have fetched
                var val = row[col];

                // null values cannot contain PII
                if (val == null || val == DBNull.Value)
                    continue;

                var badParts = Validate(col.ColumnName, val.ToString()).ToArray();

                // Pass all parts as a Failure to the destination reports
                if (badParts.Any())
                {
                    AddToReports(new Failure(badParts));
                }
                
            }
        }

        // Record progress
        DoneRows(dt.Rows.Count);
    }
    public override void Dispose()
    {
        // Once all data is finished being fetched, close the destination reports
        CloseReports();

        base.Dispose();
    }
    public override int Run()
    {
        throw new NotSupportedException();
    }
}