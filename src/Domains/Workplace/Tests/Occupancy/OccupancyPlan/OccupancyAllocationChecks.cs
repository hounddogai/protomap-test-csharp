namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationChecks
{
    public static void OccupancyAllocationStartsInIdle()
    {
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
        Check.Equal(item.Status, OccupancyAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyAllocationMovesToWarming()
    {
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
        try
        {
            item.MoveTo(OccupancyAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyAllocationStatus.Warming, "status");
    }

    public static void OccupancyAllocationRejectsMovesFromStopped()
    {
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
        item.Status = OccupancyAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(OccupancyAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OccupancyAllocationStartsInIdle();
        OccupancyAllocationMovesToWarming();
        OccupancyAllocationRejectsMovesFromStopped();
    }
}
