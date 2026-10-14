namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalStatementOfWorkTargetEngineChecks
{
    public static void GlobalStatementOfWorkTargetEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalStatementOfWorkTargetEngine.DefaultGlobalStatementOfWorkTargetEngine(Flags.NewRegistry());
        var item = GlobalStatementOfWorkTarget.NewGlobalStatementOfWorkTarget("code-789", "name-873");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalStatementOfWorkTargetEngineKeepsResultsWithinBounds();
    }
}
