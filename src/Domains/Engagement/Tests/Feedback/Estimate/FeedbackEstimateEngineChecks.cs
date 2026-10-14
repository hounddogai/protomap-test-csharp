namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FeedbackEstimateEngineChecks
{
    public static void FeedbackEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackEstimateEngine.DefaultFeedbackEstimateEngine();
        var item = FeedbackEstimate.NewFeedbackEstimate("code-695", "title-213");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackEstimateEngineKeepsResultsWithinBounds();
    }
}
