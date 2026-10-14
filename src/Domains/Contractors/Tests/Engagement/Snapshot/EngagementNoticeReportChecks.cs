namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementNoticeReportChecks
{
    public static void EngagementNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702710515);
        var items = new List<EngagementNotice>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(EngagementNotice.NewEngagementNotice($"engagement-notice-{index}", "sample"));
        }
        var report = EngagementNoticeReport.BuildEngagementNoticeReport("test", items, clock);
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
        EngagementNoticeReportCountsEachRecordOnce();
    }
}
