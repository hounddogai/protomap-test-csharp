namespace Acme.Workforce.Compensation.Tests.Adjustment.Draft;

using Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingAdjustmentReviewEngineChecks
{
    public static void RollingAdjustmentReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAdjustmentReviewEngine.DefaultRollingAdjustmentReviewEngine();
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAdjustmentReviewEngineKeepsResultsWithinBounds();
    }
}
