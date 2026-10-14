namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class GlobalBenchmarkOverrideReportChecks
{
    public static void GlobalBenchmarkOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705475793);
        var items = new List<GlobalBenchmarkOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalBenchmarkOverride.NewGlobalBenchmarkOverride($"global-benchmark-override-{index}", "sample"));
        }
        var report = GlobalBenchmarkOverrideReport.BuildGlobalBenchmarkOverrideReport("test", items, clock);
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
        GlobalBenchmarkOverrideReportCountsEachRecordOnce();
    }
}
