namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ScheduledEngagementNoticeReportChecks
{
    public static void ScheduledEngagementNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702376454);
        var items = new List<ScheduledEngagementNotice>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledEngagementNotice.NewScheduledEngagementNotice($"scheduled-engagement-notice-{index}", "sample"));
        }
        var report = ScheduledEngagementNoticeReport.BuildScheduledEngagementNoticeReport("test", items, clock);
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
        ScheduledEngagementNoticeReportCountsEachRecordOnce();
    }
}
