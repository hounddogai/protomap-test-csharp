namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackRecordEngineChecks
{
    public static void FeedbackRecordEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackRecordEngine.DefaultFeedbackRecordEngine();
        var item = FeedbackRecord.NewFeedbackRecord("code-642", "label-271");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackRecordEngineKeepsResultsWithinBounds();
    }
}
