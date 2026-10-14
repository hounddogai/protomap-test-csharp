namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackHolidayScheduleReportChecks
{
    public static void FallbackHolidayScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704739185);
        var items = new List<FallbackHolidaySchedule>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FallbackHolidaySchedule.NewFallbackHolidaySchedule($"fallback-holiday-schedule-{index}", "sample"));
        }
        var report = FallbackHolidayScheduleReport.BuildFallbackHolidayScheduleReport("test", items, clock);
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
        FallbackHolidayScheduleReportCountsEachRecordOnce();
    }
}
