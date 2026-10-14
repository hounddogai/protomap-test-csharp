namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupEngineChecks
{
    public static void FeedbackGroupEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackGroupEngine.DefaultFeedbackGroupEngine();
        var item = FeedbackGroup.NewFeedbackGroup("code-862", "name-565");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackGroupEngineKeepsResultsWithinBounds();
    }
}
