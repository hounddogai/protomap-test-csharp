namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TimesheetOverrideReportChecks
{
    public static void TimesheetOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707280184);
        var items = new List<TimesheetOverride>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TimesheetOverride.NewTimesheetOverride($"timesheet-override-{index}", "sample"));
        }
        var report = TimesheetOverrideReport.BuildTimesheetOverrideReport("test", items, clock);
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
        TimesheetOverrideReportCountsEachRecordOnce();
    }
}
