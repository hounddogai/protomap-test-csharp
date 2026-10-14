namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AccessBadgeAllocationChecks
{
    public static void AccessBadgeAllocationStartsInIdle()
    {
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("code-762", "label-772");
        Check.Equal(item.Status, AccessBadgeAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeAllocationMovesToDraining()
    {
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("code-762", "label-772");
        try
        {
            item.MoveTo(AccessBadgeAllocationStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeAllocationStatus.Draining, "status");
    }

    public static void AccessBadgeAllocationRejectsMovesFromStopped()
    {
        var item = AccessBadgeAllocation.NewAccessBadgeAllocation("code-762", "label-772");
        item.Status = AccessBadgeAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccessBadgeAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccessBadgeAllocationStartsInIdle();
        AccessBadgeAllocationMovesToDraining();
        AccessBadgeAllocationRejectsMovesFromStopped();
    }
}
