namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class AdaptiveFeedbackSummaryEngineChecks
{
    public static void AdaptiveFeedbackSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveFeedbackSummaryEngine.DefaultAdaptiveFeedbackSummaryEngine();
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("code-115", "title-965");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackSummaryEngineKeepsResultsWithinBounds();
    }
}
