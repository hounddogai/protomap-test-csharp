namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedFeedbackScoreEngineChecks
{
    public static void BatchedFeedbackScoreEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedFeedbackScoreEngine.DefaultBatchedFeedbackScoreEngine();
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedFeedbackScoreEngineKeepsResultsWithinBounds();
    }
}
