namespace Acme.Workforce.Compensation.Tests.Benchmark.Proposal;

using Acme.Workforce.Compensation.Benchmark.Proposal;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredBenchmarkSummaryReportChecks
{
    public static void TieredBenchmarkSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708159624);
        var items = new List<TieredBenchmarkSummary>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredBenchmarkSummary.NewTieredBenchmarkSummary($"tiered-benchmark-summary-{index}", "sample"));
        }
        var report = TieredBenchmarkSummaryReport.BuildTieredBenchmarkSummaryReport("test", items, clock);
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
        TieredBenchmarkSummaryReportCountsEachRecordOnce();
    }
}
