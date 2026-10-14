namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotViewChecks
{
    public static void ScheduledFloorPlanSnapshotViewShowsTheStatusInCapitals()
    {
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
        var view = ScheduledFloorPlanSnapshotView.ToScheduledFloorPlanSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledFloorPlanSnapshotView.ToScheduledFloorPlanSnapshotViews(new List<ScheduledFloorPlanSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanSnapshotViewShowsTheStatusInCapitals();
    }
}
