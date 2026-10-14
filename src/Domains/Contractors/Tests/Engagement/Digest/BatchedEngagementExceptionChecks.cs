namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BatchedEngagementExceptionChecks
{
    public static void BatchedEngagementExceptionStartsInDraft()
    {
        var item = BatchedEngagementException.NewBatchedEngagementException("code-208", "label-893");
        Check.Equal(item.Status, BatchedEngagementExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedEngagementExceptionMovesToActive()
    {
        var item = BatchedEngagementException.NewBatchedEngagementException("code-208", "label-893");
        try
        {
            item.MoveTo(BatchedEngagementExceptionStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedEngagementExceptionStatus.Active, "status");
    }

    public static void BatchedEngagementExceptionRejectsMovesFromClosed()
    {
        var item = BatchedEngagementException.NewBatchedEngagementException("code-208", "label-893");
        item.Status = BatchedEngagementExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedEngagementExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedEngagementExceptionStartsInDraft();
        BatchedEngagementExceptionMovesToActive();
        BatchedEngagementExceptionRejectsMovesFromClosed();
    }
}
