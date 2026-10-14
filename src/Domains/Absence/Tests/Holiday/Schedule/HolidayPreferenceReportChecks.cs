namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayPreferenceReportChecks
{
    public static void HolidayPreferenceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702872415);
        var items = new List<HolidayPreference>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(HolidayPreference.NewHolidayPreference($"holiday-preference-{index}", "sample"));
        }
        var report = HolidayPreferenceReport.BuildHolidayPreferenceReport("test", items, clock);
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
        HolidayPreferenceReportCountsEachRecordOnce();
    }
}
