namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedFloorPlanSnapshotViewChecks
{
    public static void BatchedFloorPlanSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("code-244", "name-817");
        var view = BatchedFloorPlanSnapshotView.ToBatchedFloorPlanSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFloorPlanSnapshotView.ToBatchedFloorPlanSnapshotViews(new List<BatchedFloorPlanSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFloorPlanSnapshotViewShowsTheStatusInCapitals();
    }
}
