namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class HolidayThresholdReportChecks
{
    public static void HolidayThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708674944);
        var items = new List<HolidayThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(HolidayThreshold.NewHolidayThreshold($"holiday-threshold-{index}", "sample"));
        }
        var report = HolidayThresholdReport.BuildHolidayThresholdReport("test", items, clock);
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
        HolidayThresholdReportCountsEachRecordOnce();
    }
}
