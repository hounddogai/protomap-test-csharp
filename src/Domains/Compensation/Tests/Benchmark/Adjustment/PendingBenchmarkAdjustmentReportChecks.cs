namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingBenchmarkAdjustmentReportChecks
{
    public static void PendingBenchmarkAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708713210);
        var items = new List<PendingBenchmarkAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment($"pending-benchmark-adjustment-{index}", "sample"));
        }
        var report = PendingBenchmarkAdjustmentReport.BuildPendingBenchmarkAdjustmentReport("test", items, clock);
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
        PendingBenchmarkAdjustmentReportCountsEachRecordOnce();
    }
}
