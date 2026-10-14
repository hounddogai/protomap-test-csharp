namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyDraft;

using Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

/// <summary>Checks of occupancy draft code, run by the project's runner.</summary>
public static class RollingOccupancyAllocationChecks
{
    public static void RollingOccupancyAllocationStartsInIdle()
    {
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("code-608", "name-94");
        Check.Equal(item.Status, RollingOccupancyAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingOccupancyAllocationMovesToReady()
    {
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("code-608", "name-94");
        try
        {
            item.MoveTo(RollingOccupancyAllocationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingOccupancyAllocationStatus.Ready, "status");
    }

    public static void RollingOccupancyAllocationRejectsMovesFromStopped()
    {
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("code-608", "name-94");
        item.Status = RollingOccupancyAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingOccupancyAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingOccupancyAllocationStartsInIdle();
        RollingOccupancyAllocationMovesToReady();
        RollingOccupancyAllocationRejectsMovesFromStopped();
    }
}
