namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentBatchEngineChecks
{
    public static void AdjustmentBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentBatchEngine.DefaultAdjustmentBatchEngine(Flags.NewRegistry());
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentBatchEngineKeepsResultsWithinBounds();
    }
}
