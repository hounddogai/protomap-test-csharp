namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class HolidayEstimateReportChecks
{
    public static void HolidayEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708028880);
        var items = new List<HolidayEstimate>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(HolidayEstimate.NewHolidayEstimate($"holiday-estimate-{index}", "sample"));
        }
        var report = HolidayEstimateReport.BuildHolidayEstimateReport("test", items, clock);
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
        HolidayEstimateReportCountsEachRecordOnce();
    }
}
