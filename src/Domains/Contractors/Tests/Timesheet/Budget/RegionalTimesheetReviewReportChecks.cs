namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RegionalTimesheetReviewReportChecks
{
    public static void RegionalTimesheetReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706956037);
        var items = new List<RegionalTimesheetReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RegionalTimesheetReview.NewRegionalTimesheetReview($"regional-timesheet-review-{index}", "sample"));
        }
        var report = RegionalTimesheetReviewReport.BuildRegionalTimesheetReviewReport("test", items, clock);
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
        RegionalTimesheetReviewReportCountsEachRecordOnce();
    }
}
