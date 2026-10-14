namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedLeaveRequestSnapshotViewChecks
{
    public static void BatchedLeaveRequestSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("code-390", "name-627");
        var view = BatchedLeaveRequestSnapshotView.ToBatchedLeaveRequestSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedLeaveRequestSnapshotView.ToBatchedLeaveRequestSnapshotViews(new List<BatchedLeaveRequestSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestSnapshotViewShowsTheStatusInCapitals();
    }
}
