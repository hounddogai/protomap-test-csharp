namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancyMilestoneChecks
{
    public static void OccupancyMilestoneStartsInDraft()
    {
        var item = OccupancyMilestone.NewOccupancyMilestone("code-397", "name-891");
        Check.Equal(item.Status, OccupancyMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyMilestoneMovesToClosed()
    {
        var item = OccupancyMilestone.NewOccupancyMilestone("code-397", "name-891");
        try
        {
            item.MoveTo(OccupancyMilestoneStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyMilestoneStatus.Closed, "status");
    }

    public static void OccupancyMilestoneRejectsMovesFromClosed()
    {
        var item = OccupancyMilestone.NewOccupancyMilestone("code-397", "name-891");
        item.Status = OccupancyMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(OccupancyMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        OccupancyMilestoneStartsInDraft();
        OccupancyMilestoneMovesToClosed();
        OccupancyMilestoneRejectsMovesFromClosed();
    }
}
