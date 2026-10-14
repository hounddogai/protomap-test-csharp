namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BatchedAccessBadgeAllocationChecks
{
    public static void BatchedAccessBadgeAllocationStartsInOpen()
    {
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("code-824", "title-597");
        Check.Equal(item.Status, BatchedAccessBadgeAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAccessBadgeAllocationMovesToArchived()
    {
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("code-824", "title-597");
        try
        {
            item.MoveTo(BatchedAccessBadgeAllocationStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAccessBadgeAllocationStatus.Archived, "status");
    }

    public static void BatchedAccessBadgeAllocationRejectsMovesFromArchived()
    {
        var item = BatchedAccessBadgeAllocation.NewBatchedAccessBadgeAllocation("code-824", "title-597");
        item.Status = BatchedAccessBadgeAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedAccessBadgeAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeAllocationStartsInOpen();
        BatchedAccessBadgeAllocationMovesToArchived();
        BatchedAccessBadgeAllocationRejectsMovesFromArchived();
    }
}
