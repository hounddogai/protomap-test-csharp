namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetMilestoneReportChecks
{
    public static void TimesheetMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709659641);
        var items = new List<TimesheetMilestone>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TimesheetMilestone.NewTimesheetMilestone($"timesheet-milestone-{index}", "sample"));
        }
        var report = TimesheetMilestoneReport.BuildTimesheetMilestoneReport("test", items, clock);
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
        TimesheetMilestoneReportCountsEachRecordOnce();
    }
}
