namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackFeedbackEntryEngineChecks
{
    public static void FallbackFeedbackEntryEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackFeedbackEntryEngine.DefaultFallbackFeedbackEntryEngine();
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("code-843", "label-281");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackFeedbackEntryEngineKeepsResultsWithinBounds();
    }
}
