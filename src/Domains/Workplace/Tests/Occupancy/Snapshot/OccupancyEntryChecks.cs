namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class OccupancyEntryChecks
{
    public static void OccupancyEntryStartsInPlanned()
    {
        var item = OccupancyEntry.NewOccupancyEntry("code-800", "name-144");
        Check.Equal(item.Status, OccupancyEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyEntryMovesToInProgress()
    {
        var item = OccupancyEntry.NewOccupancyEntry("code-800", "name-144");
        try
        {
            item.MoveTo(OccupancyEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyEntryStatus.InProgress, "status");
    }

    public static void OccupancyEntryRejectsMovesFromCancelled()
    {
        var item = OccupancyEntry.NewOccupancyEntry("code-800", "name-144");
        item.Status = OccupancyEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OccupancyEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OccupancyEntryStartsInPlanned();
        OccupancyEntryMovesToInProgress();
        OccupancyEntryRejectsMovesFromCancelled();
    }
}
