namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedBenchmarkSnapshotReportChecks
{
    public static void BatchedBenchmarkSnapshotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702985742);
        var items = new List<BatchedBenchmarkSnapshot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedBenchmarkSnapshot.NewBatchedBenchmarkSnapshot($"batched-benchmark-snapshot-{index}", "sample"));
        }
        var report = BatchedBenchmarkSnapshotReport.BuildBatchedBenchmarkSnapshotReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BatchedBenchmarkSnapshotReportCountsEachRecordOnce();
    }
}
