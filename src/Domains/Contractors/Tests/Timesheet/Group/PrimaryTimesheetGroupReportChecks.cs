namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryTimesheetGroupReportChecks
{
    public static void PrimaryTimesheetGroupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702170917);
        var items = new List<PrimaryTimesheetGroup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryTimesheetGroup.NewPrimaryTimesheetGroup($"primary-timesheet-group-{index}", "sample"));
        }
        var report = PrimaryTimesheetGroupReport.BuildPrimaryTimesheetGroupReport("test", items, clock);
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
        PrimaryTimesheetGroupReportCountsEachRecordOnce();
    }
}
