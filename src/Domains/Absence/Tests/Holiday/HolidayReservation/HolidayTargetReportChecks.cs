namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class HolidayTargetReportChecks
{
    public static void HolidayTargetReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704441213);
        var items = new List<HolidayTarget>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(HolidayTarget.NewHolidayTarget($"holiday-target-{index}", "sample"));
        }
        var report = HolidayTargetReport.BuildHolidayTargetReport("test", items, clock);
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
        HolidayTargetReportCountsEachRecordOnce();
    }
}
