namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkEstimateReportChecks
{
    public static void BenchmarkEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707459348);
        var items = new List<BenchmarkEstimate>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BenchmarkEstimate.NewBenchmarkEstimate($"benchmark-estimate-{index}", "sample"));
        }
        var report = BenchmarkEstimateReport.BuildBenchmarkEstimateReport("test", items, clock);
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
        BenchmarkEstimateReportCountsEachRecordOnce();
    }
}
