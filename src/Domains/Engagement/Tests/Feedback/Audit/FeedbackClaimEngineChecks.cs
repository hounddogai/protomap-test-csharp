namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackClaimEngineChecks
{
    public static void FeedbackClaimEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackClaimEngine.DefaultFeedbackClaimEngine();
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackClaimEngineKeepsResultsWithinBounds();
    }
}
