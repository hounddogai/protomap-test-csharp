namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayNoticeReportChecks
{
    public static void HolidayNoticeReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705527123);
        var items = new List<HolidayNotice>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayNotice.NewHolidayNotice($"holiday-notice-{index}", "sample"));
        }
        var report = HolidayNoticeReport.BuildHolidayNoticeReport("test", items, clock);
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
        HolidayNoticeReportCountsEachRecordOnce();
    }
}
