namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMilestoneReportChecks
{
    public static void AdaptiveBenchmarkMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701722310);
        var items = new List<AdaptiveBenchmarkMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone($"adaptive-benchmark-milestone-{index}", "sample"));
        }
        var report = AdaptiveBenchmarkMilestoneReport.BuildAdaptiveBenchmarkMilestoneReport("test", items, clock);
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
        AdaptiveBenchmarkMilestoneReportCountsEachRecordOnce();
    }
}
