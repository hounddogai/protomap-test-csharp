namespace Acme.Workforce.Assets.Tests.Refresh.Reservation;

using Acme.Workforce.Assets.Refresh.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RefreshAllocationChecks
{
    public static void RefreshAllocationStartsInIdle()
    {
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
        Check.Equal(item.Status, RefreshAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshAllocationMovesToWarming()
    {
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
        try
        {
            item.MoveTo(RefreshAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshAllocationStatus.Warming, "status");
    }

    public static void RefreshAllocationRejectsMovesFromStopped()
    {
        var item = RefreshAllocation.NewRefreshAllocation("code-106", "name-417");
        item.Status = RefreshAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RefreshAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RefreshAllocationStartsInIdle();
        RefreshAllocationMovesToWarming();
        RefreshAllocationRejectsMovesFromStopped();
    }
}
