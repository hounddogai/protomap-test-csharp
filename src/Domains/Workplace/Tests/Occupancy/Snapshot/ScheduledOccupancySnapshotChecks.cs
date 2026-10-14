namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledOccupancySnapshotChecks
{
    public static void ScheduledOccupancySnapshotStartsInCreated()
    {
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("code-530", "name-431");
        Check.Equal(item.Status, ScheduledOccupancySnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledOccupancySnapshotMovesToValidated()
    {
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("code-530", "name-431");
        try
        {
            item.MoveTo(ScheduledOccupancySnapshotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledOccupancySnapshotStatus.Validated, "status");
    }

    public static void ScheduledOccupancySnapshotRejectsMovesFromRetired()
    {
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("code-530", "name-431");
        item.Status = ScheduledOccupancySnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledOccupancySnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledOccupancySnapshotStartsInCreated();
        ScheduledOccupancySnapshotMovesToValidated();
        ScheduledOccupancySnapshotRejectsMovesFromRetired();
    }
}
