namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedFeedbackBatchChecks
{
    public static void BatchedFeedbackBatchStartsInDraft()
    {
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
        Check.Equal(item.Status, BatchedFeedbackBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFeedbackBatchMovesToActive()
    {
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
        try
        {
            item.MoveTo(BatchedFeedbackBatchStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFeedbackBatchStatus.Active, "status");
    }

    public static void BatchedFeedbackBatchRejectsMovesFromClosed()
    {
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
        item.Status = BatchedFeedbackBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedFeedbackBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedFeedbackBatchStartsInDraft();
        BatchedFeedbackBatchMovesToActive();
        BatchedFeedbackBatchRejectsMovesFromClosed();
    }
}
