namespace Acme.Workforce.Contractors.Tests.Timesheet.Exception;

using Acme.Workforce.Contractors.Timesheet.Exception;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TimesheetAssignmentReportChecks
{
    public static void TimesheetAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701282845);
        var items = new List<TimesheetAssignment>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TimesheetAssignment.NewTimesheetAssignment($"timesheet-assignment-{index}", "sample"));
        }
        var report = TimesheetAssignmentReport.BuildTimesheetAssignmentReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TimesheetAssignmentReportCountsEachRecordOnce();
    }
}
