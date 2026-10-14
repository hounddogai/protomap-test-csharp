namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkScoreReportChecks
{
    public static void TieredBenchmarkScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709398097);
        var items = new List<TieredBenchmarkScore>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredBenchmarkScore.NewTieredBenchmarkScore($"tiered-benchmark-score-{index}", "sample"));
        }
        var report = TieredBenchmarkScoreReport.BuildTieredBenchmarkScoreReport("test", items, clock);
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
        TieredBenchmarkScoreReportCountsEachRecordOnce();
    }
}
