namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkMilestoneReportChecks
{
    public static void BenchmarkMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709469067);
        var items = new List<BenchmarkMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BenchmarkMilestone.NewBenchmarkMilestone($"benchmark-milestone-{index}", "sample"));
        }
        var report = BenchmarkMilestoneReport.BuildBenchmarkMilestoneReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        BenchmarkMilestoneReportCountsEachRecordOnce();
    }
}
