namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkCheckpointReportChecks
{
    public static void BenchmarkCheckpointReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700446661);
        var items = new List<BenchmarkCheckpoint>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BenchmarkCheckpoint.NewBenchmarkCheckpoint($"benchmark-checkpoint-{index}", "sample"));
        }
        var report = BenchmarkCheckpointReport.BuildBenchmarkCheckpointReport("test", items, clock);
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
        BenchmarkCheckpointReportCountsEachRecordOnce();
    }
}
