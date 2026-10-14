namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class PrimaryEngagementNoticeReportChecks
{
    public static void PrimaryEngagementNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705723137);
        var items = new List<PrimaryEngagementNotice>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PrimaryEngagementNotice.NewPrimaryEngagementNotice($"primary-engagement-notice-{index}", "sample"));
        }
        var report = PrimaryEngagementNoticeReport.BuildPrimaryEngagementNoticeReport("test", items, clock);
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
        PrimaryEngagementNoticeReportCountsEachRecordOnce();
    }
}
