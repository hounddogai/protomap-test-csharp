namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackWindowEngineChecks
{
    public static void FeedbackWindowEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackWindowEngine.DefaultFeedbackWindowEngine();
        var item = FeedbackWindow.NewFeedbackWindow("code-69", "title-350");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackWindowEngineKeepsResultsWithinBounds();
    }
}
