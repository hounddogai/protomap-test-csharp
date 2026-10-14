namespace Acme.Workforce.Contractors.Tests.Engagement.Proposal;

using Acme.Workforce.Contractors.Engagement.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class EngagementAllocationChecks
{
    public static void EngagementAllocationStartsInIdle()
    {
        var item = EngagementAllocation.NewEngagementAllocation("code-34", "label-551");
        Check.Equal(item.Status, EngagementAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementAllocationMovesToWarming()
    {
        var item = EngagementAllocation.NewEngagementAllocation("code-34", "label-551");
        try
        {
            item.MoveTo(EngagementAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementAllocationStatus.Warming, "status");
    }

    public static void EngagementAllocationRejectsMovesFromStopped()
    {
        var item = EngagementAllocation.NewEngagementAllocation("code-34", "label-551");
        item.Status = EngagementAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(EngagementAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        EngagementAllocationStartsInIdle();
        EngagementAllocationMovesToWarming();
        EngagementAllocationRejectsMovesFromStopped();
    }
}
