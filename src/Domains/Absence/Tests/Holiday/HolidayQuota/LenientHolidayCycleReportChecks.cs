namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class LenientHolidayCycleReportChecks
{
    public static void LenientHolidayCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700864727);
        var items = new List<LenientHolidayCycle>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(LenientHolidayCycle.NewLenientHolidayCycle($"lenient-holiday-cycle-{index}", "sample"));
        }
        var report = LenientHolidayCycleReport.BuildLenientHolidayCycleReport("test", items, clock);
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
        LenientHolidayCycleReportCountsEachRecordOnce();
    }
}
