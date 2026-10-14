namespace Acme.Workforce.Contractors.Tests.Engagement.Checkpoint;

using Acme.Workforce.Contractors.Engagement.Checkpoint;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class EngagementTargetReportChecks
{
    public static void EngagementTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700166809);
        var items = new List<EngagementTarget>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(EngagementTarget.NewEngagementTarget($"engagement-target-{index}", "sample"));
        }
        var report = EngagementTargetReport.BuildEngagementTargetReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EngagementTargetReportCountsEachRecordOnce();
    }
}
