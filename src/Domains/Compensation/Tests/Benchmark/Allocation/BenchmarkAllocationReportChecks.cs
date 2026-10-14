namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkAllocationReportChecks
{
    public static void BenchmarkAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704499815);
        var items = new List<BenchmarkAllocation>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BenchmarkAllocation.NewBenchmarkAllocation($"benchmark-allocation-{index}", "sample"));
        }
        var report = BenchmarkAllocationReport.BuildBenchmarkAllocationReport("test", items, clock);
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
        BenchmarkAllocationReportCountsEachRecordOnce();
    }
}
