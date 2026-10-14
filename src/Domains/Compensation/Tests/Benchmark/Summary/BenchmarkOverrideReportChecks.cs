namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkOverrideReportChecks
{
    public static void BenchmarkOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701738317);
        var items = new List<BenchmarkOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BenchmarkOverride.NewBenchmarkOverride($"benchmark-override-{index}", "sample"));
        }
        var report = BenchmarkOverrideReport.BuildBenchmarkOverrideReport("test", items, clock);
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
        BenchmarkOverrideReportCountsEachRecordOnce();
    }
}
