namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredTimesheetProfileReportChecks
{
    public static void TieredTimesheetProfileReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709242954);
        var items = new List<TieredTimesheetProfile>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(TieredTimesheetProfile.NewTieredTimesheetProfile($"tiered-timesheet-profile-{index}", "sample"));
        }
        var report = TieredTimesheetProfileReport.BuildTieredTimesheetProfileReport("test", items, clock);
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
        TieredTimesheetProfileReportCountsEachRecordOnce();
    }
}
