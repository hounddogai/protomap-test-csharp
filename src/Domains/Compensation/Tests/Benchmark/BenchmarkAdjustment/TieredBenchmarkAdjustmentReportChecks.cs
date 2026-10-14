namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class TieredBenchmarkAdjustmentReportChecks
{
    public static void TieredBenchmarkAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709055575);
        var items = new List<TieredBenchmarkAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment($"tiered-benchmark-adjustment-{index}", "sample"));
        }
        var report = TieredBenchmarkAdjustmentReport.BuildTieredBenchmarkAdjustmentReport("test", items, clock);
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
        TieredBenchmarkAdjustmentReportCountsEachRecordOnce();
    }
}
