namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BatchedRefreshWindowChecks
{
    public static void BatchedRefreshWindowStartsInProposed()
    {
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("code-120", "label-347");
        Check.Equal(item.Status, BatchedRefreshWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRefreshWindowMovesToRejected()
    {
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("code-120", "label-347");
        try
        {
            item.MoveTo(BatchedRefreshWindowStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRefreshWindowStatus.Rejected, "status");
    }

    public static void BatchedRefreshWindowRejectsMovesFromWithdrawn()
    {
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("code-120", "label-347");
        item.Status = BatchedRefreshWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedRefreshWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedRefreshWindowStartsInProposed();
        BatchedRefreshWindowMovesToRejected();
        BatchedRefreshWindowRejectsMovesFromWithdrawn();
    }
}
