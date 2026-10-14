namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackRecordChecks
{
    public static void FeedbackRecordStartsInNew()
    {
        var item = FeedbackRecord.NewFeedbackRecord("code-642", "label-271");
        Check.Equal(item.Status, FeedbackRecordStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackRecordMovesToQueued()
    {
        var item = FeedbackRecord.NewFeedbackRecord("code-642", "label-271");
        try
        {
            item.MoveTo(FeedbackRecordStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackRecordStatus.Queued, "status");
    }

    public static void FeedbackRecordRejectsMovesFromFailed()
    {
        var item = FeedbackRecord.NewFeedbackRecord("code-642", "label-271");
        item.Status = FeedbackRecordStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackRecordStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackRecordStartsInNew();
        FeedbackRecordMovesToQueued();
        FeedbackRecordRejectsMovesFromFailed();
    }
}
