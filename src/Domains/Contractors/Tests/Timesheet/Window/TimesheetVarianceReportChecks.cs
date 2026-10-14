namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetVarianceReportChecks
{
    public static void TimesheetVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701882141);
        var items = new List<TimesheetVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TimesheetVariance.NewTimesheetVariance($"timesheet-variance-{index}", "sample"));
        }
        var report = TimesheetVarianceReport.BuildTimesheetVarianceReport("test", items, clock);
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
        TimesheetVarianceReportCountsEachRecordOnce();
    }
}
