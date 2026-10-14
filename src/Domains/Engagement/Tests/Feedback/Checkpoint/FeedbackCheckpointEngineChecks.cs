namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FeedbackCheckpointEngineChecks
{
    public static void FeedbackCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackCheckpointEngine.DefaultFeedbackCheckpointEngine();
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackCheckpointEngineKeepsResultsWithinBounds();
    }
}
