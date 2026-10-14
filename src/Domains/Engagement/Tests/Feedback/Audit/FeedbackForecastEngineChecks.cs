namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackForecastEngineChecks
{
    public static void FeedbackForecastEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackForecastEngine.DefaultFeedbackForecastEngine(Flags.NewRegistry());
        var item = FeedbackForecast.NewFeedbackForecast("code-987", "label-830");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackForecastEngineKeepsResultsWithinBounds();
    }
}
