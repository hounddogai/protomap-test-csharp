namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentChecks
{
    public static void BatchedRefreshAssignmentStartsInNew()
    {
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
        Check.Equal(item.Status, BatchedRefreshAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRefreshAssignmentMovesToQueued()
    {
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
        try
        {
            item.MoveTo(BatchedRefreshAssignmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRefreshAssignmentStatus.Queued, "status");
    }

    public static void BatchedRefreshAssignmentRejectsMovesFromFailed()
    {
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
        item.Status = BatchedRefreshAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedRefreshAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedRefreshAssignmentStartsInNew();
        BatchedRefreshAssignmentMovesToQueued();
        BatchedRefreshAssignmentRejectsMovesFromFailed();
    }
}
