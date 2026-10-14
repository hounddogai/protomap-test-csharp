namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementMilestoneReportChecks
{
    public static void EngagementMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702615069);
        var items = new List<EngagementMilestone>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementMilestone.NewEngagementMilestone($"engagement-milestone-{index}", "sample"));
        }
        var report = EngagementMilestoneReport.BuildEngagementMilestoneReport("test", items, clock);
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
        EngagementMilestoneReportCountsEachRecordOnce();
    }
}
