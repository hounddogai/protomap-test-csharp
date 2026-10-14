namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkCheckpointEngineChecks
{
    public static void StatementOfWorkCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkCheckpointEngine.DefaultStatementOfWorkCheckpointEngine(Flags.NewRegistry());
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkCheckpointEngineKeepsResultsWithinBounds();
    }
}
