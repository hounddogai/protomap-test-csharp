namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BatchedAdjustmentScheduleEngineChecks
{
    public static void BatchedAdjustmentScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAdjustmentScheduleEngine.DefaultBatchedAdjustmentScheduleEngine(Flags.NewRegistry());
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAdjustmentScheduleEngineKeepsResultsWithinBounds();
    }
}
