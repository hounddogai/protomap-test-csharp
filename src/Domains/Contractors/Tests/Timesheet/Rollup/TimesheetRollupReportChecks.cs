namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetRollupReportChecks
{
    public static void TimesheetRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705413334);
        var items = new List<TimesheetRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TimesheetRollup.NewTimesheetRollup($"timesheet-rollup-{index}", "sample"));
        }
        var report = TimesheetRollupReport.BuildTimesheetRollupReport("test", items, clock);
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
        TimesheetRollupReportCountsEachRecordOnce();
    }
}
