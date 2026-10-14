namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class BatchedReorgSnapshotReportChecks
{
    public static void BatchedReorgSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704912530);
        var items = new List<BatchedReorgSnapshot>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BatchedReorgSnapshot.NewBatchedReorgSnapshot($"batched-reorg-snapshot-{index}", "sample"));
        }
        var report = BatchedReorgSnapshotReport.BuildBatchedReorgSnapshotReport("test", items, clock);
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
        BatchedReorgSnapshotReportCountsEachRecordOnce();
    }
}
