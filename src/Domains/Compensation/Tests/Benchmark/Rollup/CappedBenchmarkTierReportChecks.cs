namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedBenchmarkTierReportChecks
{
    public static void CappedBenchmarkTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700760820);
        var items = new List<CappedBenchmarkTier>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(CappedBenchmarkTier.NewCappedBenchmarkTier($"capped-benchmark-tier-{index}", "sample"));
        }
        var report = CappedBenchmarkTierReport.BuildCappedBenchmarkTierReport("test", items, clock);
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
        CappedBenchmarkTierReportCountsEachRecordOnce();
    }
}
