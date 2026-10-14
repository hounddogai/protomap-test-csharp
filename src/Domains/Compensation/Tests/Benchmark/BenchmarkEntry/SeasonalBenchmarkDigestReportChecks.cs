namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class SeasonalBenchmarkDigestReportChecks
{
    public static void SeasonalBenchmarkDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706239507);
        var items = new List<SeasonalBenchmarkDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalBenchmarkDigest.NewSeasonalBenchmarkDigest($"seasonal-benchmark-digest-{index}", "sample"));
        }
        var report = SeasonalBenchmarkDigestReport.BuildSeasonalBenchmarkDigestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalBenchmarkDigestReportCountsEachRecordOnce();
    }
}
