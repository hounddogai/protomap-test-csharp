namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackMilestoneEngineChecks
{
    public static void FeedbackMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackMilestoneEngine.DefaultFeedbackMilestoneEngine(Flags.NewRegistry());
        var item = FeedbackMilestone.NewFeedbackMilestone("code-416", "title-321");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackMilestoneEngineKeepsResultsWithinBounds();
    }
}
