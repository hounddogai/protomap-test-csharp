namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class FeedbackThresholdEngineChecks
{
    public static void FeedbackThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackThresholdEngine.DefaultFeedbackThresholdEngine(Flags.NewRegistry());
        var item = FeedbackThreshold.NewFeedbackThreshold("code-493", "label-298");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackThresholdEngineKeepsResultsWithinBounds();
    }
}
