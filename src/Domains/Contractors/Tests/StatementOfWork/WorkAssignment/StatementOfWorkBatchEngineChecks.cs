namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkAssignment;

using Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of work assignment code, run by the project's runner.</summary>
public static class StatementOfWorkBatchEngineChecks
{
    public static void StatementOfWorkBatchEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkBatchEngine.DefaultStatementOfWorkBatchEngine(Flags.NewRegistry());
        var item = StatementOfWorkBatch.NewStatementOfWorkBatch("code-514", "label-820");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkBatchEngineKeepsResultsWithinBounds();
    }
}
