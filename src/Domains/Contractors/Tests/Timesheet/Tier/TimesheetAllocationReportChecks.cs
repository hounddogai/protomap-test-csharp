namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TimesheetAllocationReportChecks
{
    public static void TimesheetAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702681416);
        var items = new List<TimesheetAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TimesheetAllocation.NewTimesheetAllocation($"timesheet-allocation-{index}", "sample"));
        }
        var report = TimesheetAllocationReport.BuildTimesheetAllocationReport("test", items, clock);
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
        TimesheetAllocationReportCountsEachRecordOnce();
    }
}
