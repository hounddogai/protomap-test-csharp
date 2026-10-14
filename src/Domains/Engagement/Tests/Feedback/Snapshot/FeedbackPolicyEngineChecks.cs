namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackPolicyEngineChecks
{
    public static void FeedbackPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackPolicyEngine.DefaultFeedbackPolicyEngine();
        var item = FeedbackPolicy.NewFeedbackPolicy("code-489", "title-886");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackPolicyEngineKeepsResultsWithinBounds();
    }
}
