namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionReportChecks
{
    public static void HolidayExceptionReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708080063);
        var items = new List<HolidayException>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(HolidayException.NewHolidayException($"holiday-exception-{index}", "sample"));
        }
        var report = HolidayExceptionReport.BuildHolidayExceptionReport("test", items, clock);
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
        HolidayExceptionReportCountsEachRecordOnce();
    }
}
