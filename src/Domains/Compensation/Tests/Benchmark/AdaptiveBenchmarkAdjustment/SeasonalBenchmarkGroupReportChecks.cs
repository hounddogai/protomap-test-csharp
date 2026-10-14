namespace Acme.Workforce.Compensation.Tests.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adaptive benchmark adjustment code, run by the project's runner.</summary>
public static class SeasonalBenchmarkGroupReportChecks
{
    public static void SeasonalBenchmarkGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701037375);
        var items = new List<SeasonalBenchmarkGroup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SeasonalBenchmarkGroup.NewSeasonalBenchmarkGroup($"seasonal-benchmark-group-{index}", "sample"));
        }
        var report = SeasonalBenchmarkGroupReport.BuildSeasonalBenchmarkGroupReport("test", items, clock);
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
        SeasonalBenchmarkGroupReportCountsEachRecordOnce();
    }
}
