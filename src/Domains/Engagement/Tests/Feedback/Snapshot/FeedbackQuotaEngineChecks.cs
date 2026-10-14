namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackQuotaEngineChecks
{
    public static void FeedbackQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackQuotaEngine.DefaultFeedbackQuotaEngine(Flags.NewRegistry());
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackQuotaEngineKeepsResultsWithinBounds();
    }
}
