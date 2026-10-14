namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationReportChecks
{
    public static void ManualBenchmarkAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703164489);
        var items = new List<ManualBenchmarkAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ManualBenchmarkAllocation.NewManualBenchmarkAllocation($"manual-benchmark-allocation-{index}", "sample"));
        }
        var report = ManualBenchmarkAllocationReport.BuildManualBenchmarkAllocationReport("test", items, clock);
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
        ManualBenchmarkAllocationReportCountsEachRecordOnce();
    }
}
