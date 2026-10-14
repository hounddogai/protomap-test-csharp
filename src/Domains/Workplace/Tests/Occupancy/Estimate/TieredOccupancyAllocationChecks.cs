namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredOccupancyAllocationChecks
{
    public static void TieredOccupancyAllocationStartsInRequested()
    {
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("code-220", "name-985");
        Check.Equal(item.Status, TieredOccupancyAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredOccupancyAllocationMovesToGranted()
    {
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("code-220", "name-985");
        try
        {
            item.MoveTo(TieredOccupancyAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredOccupancyAllocationStatus.Granted, "status");
    }

    public static void TieredOccupancyAllocationRejectsMovesFromExpired()
    {
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("code-220", "name-985");
        item.Status = TieredOccupancyAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredOccupancyAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredOccupancyAllocationStartsInRequested();
        TieredOccupancyAllocationMovesToGranted();
        TieredOccupancyAllocationRejectsMovesFromExpired();
    }
}
