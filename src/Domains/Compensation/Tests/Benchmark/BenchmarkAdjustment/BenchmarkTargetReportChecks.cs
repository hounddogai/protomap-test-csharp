namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class BenchmarkTargetReportChecks
{
    public static void BenchmarkTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704610215);
        var items = new List<BenchmarkTarget>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BenchmarkTarget.NewBenchmarkTarget($"benchmark-target-{index}", "sample"));
        }
        var report = BenchmarkTargetReport.BuildBenchmarkTargetReport("test", items, clock);
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
        BenchmarkTargetReportCountsEachRecordOnce();
    }
}
