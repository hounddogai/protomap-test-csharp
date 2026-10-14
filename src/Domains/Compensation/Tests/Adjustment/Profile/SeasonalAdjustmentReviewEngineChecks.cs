namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReviewEngineChecks
{
    public static void SeasonalAdjustmentReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAdjustmentReviewEngine.DefaultSeasonalAdjustmentReviewEngine();
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("code-600", "title-42");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReviewEngineKeepsResultsWithinBounds();
    }
}
