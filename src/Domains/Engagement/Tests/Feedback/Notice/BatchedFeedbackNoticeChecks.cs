namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedFeedbackNoticeChecks
{
    public static void BatchedFeedbackNoticeStartsInDraft()
    {
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
        Check.Equal(item.Status, BatchedFeedbackNoticeStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFeedbackNoticeMovesToPending()
    {
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
        try
        {
            item.MoveTo(BatchedFeedbackNoticeStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFeedbackNoticeStatus.Pending, "status");
    }

    public static void BatchedFeedbackNoticeRejectsMovesFromClosed()
    {
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
        item.Status = BatchedFeedbackNoticeStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedFeedbackNoticeStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedFeedbackNoticeStartsInDraft();
        BatchedFeedbackNoticeMovesToPending();
        BatchedFeedbackNoticeRejectsMovesFromClosed();
    }
}
