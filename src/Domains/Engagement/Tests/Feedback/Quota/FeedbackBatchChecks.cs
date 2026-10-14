namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackBatchChecks
{
    public static void FeedbackBatchStartsInProposed()
    {
        var item = FeedbackBatch.NewFeedbackBatch("code-598", "label-336");
        Check.Equal(item.Status, FeedbackBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackBatchMovesToWithdrawn()
    {
        var item = FeedbackBatch.NewFeedbackBatch("code-598", "label-336");
        try
        {
            item.MoveTo(FeedbackBatchStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackBatchStatus.Withdrawn, "status");
    }

    public static void FeedbackBatchRejectsMovesFromWithdrawn()
    {
        var item = FeedbackBatch.NewFeedbackBatch("code-598", "label-336");
        item.Status = FeedbackBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FeedbackBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FeedbackBatchStartsInProposed();
        FeedbackBatchMovesToWithdrawn();
        FeedbackBatchRejectsMovesFromWithdrawn();
    }
}
