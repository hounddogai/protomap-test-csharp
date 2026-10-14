namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackBatchEngineChecks
{
    public static void FeedbackBatchEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackBatchEngine.DefaultFeedbackBatchEngine();
        var item = FeedbackBatch.NewFeedbackBatch("code-598", "label-336");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackBatchEngineKeepsResultsWithinBounds();
    }
}
