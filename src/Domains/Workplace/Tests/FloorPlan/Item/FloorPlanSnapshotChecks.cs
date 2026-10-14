namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSnapshotChecks
{
    public static void FloorPlanSnapshotStartsInOpen()
    {
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("code-506", "title-55");
        Check.Equal(item.Status, FloorPlanSnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FloorPlanSnapshotMovesToResolved()
    {
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("code-506", "title-55");
        try
        {
            item.MoveTo(FloorPlanSnapshotStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FloorPlanSnapshotStatus.Resolved, "status");
    }

    public static void FloorPlanSnapshotRejectsMovesFromArchived()
    {
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("code-506", "title-55");
        item.Status = FloorPlanSnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(FloorPlanSnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        FloorPlanSnapshotStartsInOpen();
        FloorPlanSnapshotMovesToResolved();
        FloorPlanSnapshotRejectsMovesFromArchived();
    }
}
