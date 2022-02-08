using IsIdentifiable.Options;
using IsIdentifiable.Runners;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;

namespace IsIdentifiablePlugin;

internal class ExecuteCommandRunIsIdentifiable : BasicCommandExecution
{
    public ExecuteCommandRunIsIdentifiable(IBasicActivateItems activator, ICatalogue catalogue) : base(activator)
    {

    }
    public override void Execute()
    {
        var opts = new IsIdentifiableRelationalDatabaseOptions()
        {
            
        };
        var runner = new DatabaseRunner(opts);
        runner.Run();
    }
}
