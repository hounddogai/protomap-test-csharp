namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedMeritCycleReviewEngineChecks
{
    public static void BatchedMeritCycleReviewEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedMeritCycleReviewEngine.DefaultBatchedMeritCycleReviewEngine(Flags.NewRegistry());
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("code-492", "title-864");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedMeritCycleReviewEngineKeepsResultsWithinBounds();
    }
}
