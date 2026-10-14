namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedFeedbackNoticeEngineChecks
{
    public static void BatchedFeedbackNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedFeedbackNoticeEngine.DefaultBatchedFeedbackNoticeEngine(Flags.NewRegistry());
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedFeedbackNoticeEngineKeepsResultsWithinBounds();
    }
}
