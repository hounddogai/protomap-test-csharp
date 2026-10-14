namespace Acme.Workforce.Engagement.Tests.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledFeedbackBaselineEngineChecks
{
    public static void ScheduledFeedbackBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFeedbackBaselineEngine.DefaultScheduledFeedbackBaselineEngine(Flags.NewRegistry());
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFeedbackBaselineEngineKeepsResultsWithinBounds();
    }
}
