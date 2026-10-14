namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PendingFeedbackMilestoneEngineChecks
{
    public static void PendingFeedbackMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = PendingFeedbackMilestoneEngine.DefaultPendingFeedbackMilestoneEngine();
        var item = PendingFeedbackMilestone.NewPendingFeedbackMilestone("code-51", "name-723");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingFeedbackMilestoneEngineKeepsResultsWithinBounds();
    }
}
