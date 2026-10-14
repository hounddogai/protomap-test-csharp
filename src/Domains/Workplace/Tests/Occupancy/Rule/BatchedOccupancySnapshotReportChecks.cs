namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedOccupancySnapshotReportChecks
{
    public static void BatchedOccupancySnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709189198);
        var items = new List<BatchedOccupancySnapshot>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BatchedOccupancySnapshot.NewBatchedOccupancySnapshot($"batched-occupancy-snapshot-{index}", "sample"));
        }
        var report = BatchedOccupancySnapshotReport.BuildBatchedOccupancySnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BatchedOccupancySnapshotReportCountsEachRecordOnce();
    }
}
