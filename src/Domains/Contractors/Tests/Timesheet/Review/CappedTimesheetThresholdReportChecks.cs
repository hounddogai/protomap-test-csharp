namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedTimesheetThresholdReportChecks
{
    public static void CappedTimesheetThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708561821);
        var items = new List<CappedTimesheetThreshold>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(CappedTimesheetThreshold.NewCappedTimesheetThreshold($"capped-timesheet-threshold-{index}", "sample"));
        }
        var report = CappedTimesheetThresholdReport.BuildCappedTimesheetThresholdReport("test", items, clock);
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
        CappedTimesheetThresholdReportCountsEachRecordOnce();
    }
}
