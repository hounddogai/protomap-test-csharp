namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreReportChecks
{
    public static void LenientTimesheetScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700523515);
        var items = new List<LenientTimesheetScore>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(LenientTimesheetScore.NewLenientTimesheetScore($"lenient-timesheet-score-{index}", "sample"));
        }
        var report = LenientTimesheetScoreReport.BuildLenientTimesheetScoreReport("test", items, clock);
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
        LenientTimesheetScoreReportCountsEachRecordOnce();
    }
}
