namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedFeedbackBaselineEngineChecks
{
    public static void BatchedFeedbackBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedFeedbackBaselineEngine.DefaultBatchedFeedbackBaselineEngine();
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("code-949", "title-887");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedFeedbackBaselineEngineKeepsResultsWithinBounds();
    }
}
