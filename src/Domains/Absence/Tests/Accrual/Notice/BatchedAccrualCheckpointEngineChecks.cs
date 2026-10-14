namespace Acme.Workforce.Absence.Tests.Accrual.Notice;

using Acme.Workforce.Absence.Accrual.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedAccrualCheckpointEngineChecks
{
    public static void BatchedAccrualCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAccrualCheckpointEngine.DefaultBatchedAccrualCheckpointEngine(Flags.NewRegistry());
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("code-107", "label-375");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAccrualCheckpointEngineKeepsResultsWithinBounds();
    }
}
