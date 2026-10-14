namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSnapshotViewChecks
{
    public static void FloorPlanSnapshotViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("code-506", "title-55");
        var view = FloorPlanSnapshotView.ToFloorPlanSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanSnapshotView.ToFloorPlanSnapshotViews(new List<FloorPlanSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanSnapshotViewShowsTheStatusInCapitals();
    }
}
