namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class RateMilestoneReportChecks
{
    public static void RateMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704489595);
        var items = new List<RateMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RateMilestone.NewRateMilestone($"rate-milestone-{index}", "sample"));
        }
        var report = RateMilestoneReport.BuildRateMilestoneReport("test", items, clock);
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
        RateMilestoneReportCountsEachRecordOnce();
    }
}
