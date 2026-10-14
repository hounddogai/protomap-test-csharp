namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackBaselineEngineChecks
{
    public static void FeedbackBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackBaselineEngine.DefaultFeedbackBaselineEngine(Flags.NewRegistry());
        var item = FeedbackBaseline.NewFeedbackBaseline("code-670", "title-820");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackBaselineEngineKeepsResultsWithinBounds();
    }
}
