namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetThresholdReportChecks
{
    public static void TimesheetThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702917125);
        var items = new List<TimesheetThreshold>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TimesheetThreshold.NewTimesheetThreshold($"timesheet-threshold-{index}", "sample"));
        }
        var report = TimesheetThresholdReport.BuildTimesheetThresholdReport("test", items, clock);
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
        TimesheetThresholdReportCountsEachRecordOnce();
    }
}
