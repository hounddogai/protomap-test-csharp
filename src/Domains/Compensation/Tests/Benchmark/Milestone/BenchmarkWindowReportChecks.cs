namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowReportChecks
{
    public static void BenchmarkWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700298406);
        var items = new List<BenchmarkWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BenchmarkWindow.NewBenchmarkWindow($"benchmark-window-{index}", "sample"));
        }
        var report = BenchmarkWindowReport.BuildBenchmarkWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BenchmarkWindowReportCountsEachRecordOnce();
    }
}
