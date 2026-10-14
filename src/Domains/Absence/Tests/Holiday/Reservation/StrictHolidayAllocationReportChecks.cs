namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictHolidayAllocationReportChecks
{
    public static void StrictHolidayAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704777023);
        var items = new List<StrictHolidayAllocation>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(StrictHolidayAllocation.NewStrictHolidayAllocation($"strict-holiday-allocation-{index}", "sample"));
        }
        var report = StrictHolidayAllocationReport.BuildStrictHolidayAllocationReport("test", items, clock);
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
        StrictHolidayAllocationReportCountsEachRecordOnce();
    }
}
