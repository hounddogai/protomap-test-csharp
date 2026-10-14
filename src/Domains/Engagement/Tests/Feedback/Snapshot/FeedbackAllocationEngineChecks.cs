namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackAllocationEngineChecks
{
    public static void FeedbackAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackAllocationEngine.DefaultFeedbackAllocationEngine(Flags.NewRegistry());
        var item = FeedbackAllocation.NewFeedbackAllocation("code-996", "title-266");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackAllocationEngineKeepsResultsWithinBounds();
    }
}
