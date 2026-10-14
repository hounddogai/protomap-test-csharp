namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotChecks
{
    public static void ScheduledFloorPlanSnapshotStartsInNew()
    {
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
        Check.Equal(item.Status, ScheduledFloorPlanSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledFloorPlanSnapshotMovesToQueued()
    {
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
        try
        {
            item.MoveTo(ScheduledFloorPlanSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledFloorPlanSnapshotStatus.Queued, "status");
    }

    public static void ScheduledFloorPlanSnapshotRejectsMovesFromFailed()
    {
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
        item.Status = ScheduledFloorPlanSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledFloorPlanSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanSnapshotStartsInNew();
        ScheduledFloorPlanSnapshotMovesToQueued();
        ScheduledFloorPlanSnapshotRejectsMovesFromFailed();
    }
}
