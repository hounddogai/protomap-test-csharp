namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class WeightedHolidayMappingReportChecks
{
    public static void WeightedHolidayMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704421717);
        var items = new List<WeightedHolidayMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedHolidayMapping.NewWeightedHolidayMapping($"weighted-holiday-mapping-{index}", "sample"));
        }
        var report = WeightedHolidayMappingReport.BuildWeightedHolidayMappingReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        WeightedHolidayMappingReportCountsEachRecordOnce();
    }
}
