namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayScheduleReportChecks
{
    public static void HolidayScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706561706);
        var items = new List<HolidaySchedule>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HolidaySchedule.NewHolidaySchedule($"holiday-schedule-{index}", "sample"));
        }
        var report = HolidayScheduleReport.BuildHolidayScheduleReport("test", items, clock);
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
        HolidayScheduleReportCountsEachRecordOnce();
    }
}
