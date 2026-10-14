namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class RollingOccupancyAdjustmentChecks
{
    public static void RollingOccupancyAdjustmentStartsInOpen()
    {
        var item = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment("code-638", "label-392");
        Check.Equal(item.Status, RollingOccupancyAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingOccupancyAdjustmentMovesToReopened()
    {
        var item = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment("code-638", "label-392");
        try
        {
            item.MoveTo(RollingOccupancyAdjustmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingOccupancyAdjustmentStatus.Reopened, "status");
    }

    public static void RollingOccupancyAdjustmentRejectsMovesFromArchived()
    {
        var item = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment("code-638", "label-392");
        item.Status = RollingOccupancyAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingOccupancyAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingOccupancyAdjustmentStartsInOpen();
        RollingOccupancyAdjustmentMovesToReopened();
        RollingOccupancyAdjustmentRejectsMovesFromArchived();
    }
}
