namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackEstimateEngineChecks
{
    public static void SeasonalFeedbackEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalFeedbackEstimateEngine.DefaultSeasonalFeedbackEstimateEngine(Flags.NewRegistry());
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("code-815", "label-859");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalFeedbackEstimateEngineKeepsResultsWithinBounds();
    }
}
