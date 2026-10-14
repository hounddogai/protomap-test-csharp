namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalHolidayAdjustmentReportChecks
{
    public static void RegionalHolidayAdjustmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701283595);
        var items = new List<RegionalHolidayAdjustment>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RegionalHolidayAdjustment.NewRegionalHolidayAdjustment($"regional-holiday-adjustment-{index}", "sample"));
        }
        var report = RegionalHolidayAdjustmentReport.BuildRegionalHolidayAdjustmentReport("test", items, clock);
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
        RegionalHolidayAdjustmentReportCountsEachRecordOnce();
    }
}
