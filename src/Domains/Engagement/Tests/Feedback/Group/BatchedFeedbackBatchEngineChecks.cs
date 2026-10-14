namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedFeedbackBatchEngineChecks
{
    public static void BatchedFeedbackBatchEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedFeedbackBatchEngine.DefaultBatchedFeedbackBatchEngine(Flags.NewRegistry());
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedFeedbackBatchEngineKeepsResultsWithinBounds();
    }
}
