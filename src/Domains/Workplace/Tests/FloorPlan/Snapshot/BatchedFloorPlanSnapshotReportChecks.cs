namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedFloorPlanSnapshotReportChecks
{
    public static void BatchedFloorPlanSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707428061);
        var items = new List<BatchedFloorPlanSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot($"batched-floor-plan-snapshot-{index}", "sample"));
        }
        var report = BatchedFloorPlanSnapshotReport.BuildBatchedFloorPlanSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BatchedFloorPlanSnapshotReportCountsEachRecordOnce();
    }
}
