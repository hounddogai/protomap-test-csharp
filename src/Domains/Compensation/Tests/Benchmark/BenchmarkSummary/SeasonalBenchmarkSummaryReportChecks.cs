namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class SeasonalBenchmarkSummaryReportChecks
{
    public static void SeasonalBenchmarkSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704295436);
        var items = new List<SeasonalBenchmarkSummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalBenchmarkSummary.NewSeasonalBenchmarkSummary($"seasonal-benchmark-summary-{index}", "sample"));
        }
        var report = SeasonalBenchmarkSummaryReport.BuildSeasonalBenchmarkSummaryReport("test", items, clock);
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
        SeasonalBenchmarkSummaryReportCountsEachRecordOnce();
    }
}
