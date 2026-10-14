namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkProfileReportChecks
{
    public static void TieredBenchmarkProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700398287);
        var items = new List<TieredBenchmarkProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredBenchmarkProfile.NewTieredBenchmarkProfile($"tiered-benchmark-profile-{index}", "sample"));
        }
        var report = TieredBenchmarkProfileReport.BuildTieredBenchmarkProfileReport("test", items, clock);
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
        TieredBenchmarkProfileReportCountsEachRecordOnce();
    }
}
