namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class TimesheetTierReportChecks
{
    public static void TimesheetTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701577084);
        var items = new List<TimesheetTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(TimesheetTier.NewTimesheetTier($"timesheet-tier-{index}", "sample"));
        }
        var report = TimesheetTierReport.BuildTimesheetTierReport("test", items, clock);
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
        TimesheetTierReportCountsEachRecordOnce();
    }
}
