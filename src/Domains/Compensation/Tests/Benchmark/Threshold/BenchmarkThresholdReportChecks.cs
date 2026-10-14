namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkThresholdReportChecks
{
    public static void BenchmarkThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708932691);
        var items = new List<BenchmarkThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BenchmarkThreshold.NewBenchmarkThreshold($"benchmark-threshold-{index}", "sample"));
        }
        var report = BenchmarkThresholdReport.BuildBenchmarkThresholdReport("test", items, clock);
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
        BenchmarkThresholdReportCountsEachRecordOnce();
    }
}
