namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ManualAccessBadgeNoticeReportChecks
{
    public static void ManualAccessBadgeNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708425127);
        var items = new List<ManualAccessBadgeNotice>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ManualAccessBadgeNotice.NewManualAccessBadgeNotice($"manual-access-badge-notice-{index}", "sample"));
        }
        var report = ManualAccessBadgeNoticeReport.BuildManualAccessBadgeNoticeReport("test", items, clock);
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
        ManualAccessBadgeNoticeReportCountsEachRecordOnce();
    }
}
