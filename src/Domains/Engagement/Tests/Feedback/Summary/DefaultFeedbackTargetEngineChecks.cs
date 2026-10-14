namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultFeedbackTargetEngineChecks
{
    public static void DefaultFeedbackTargetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultFeedbackTargetEngine.DefaultDefaultFeedbackTargetEngine();
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("code-712", "title-483");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultFeedbackTargetEngineKeepsResultsWithinBounds();
    }
}
