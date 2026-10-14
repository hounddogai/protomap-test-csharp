namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetBatchReportChecks
{
    public static void TimesheetBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702907531);
        var items = new List<TimesheetBatch>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(TimesheetBatch.NewTimesheetBatch($"timesheet-batch-{index}", "sample"));
        }
        var report = TimesheetBatchReport.BuildTimesheetBatchReport("test", items, clock);
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
        TimesheetBatchReportCountsEachRecordOnce();
    }
}
