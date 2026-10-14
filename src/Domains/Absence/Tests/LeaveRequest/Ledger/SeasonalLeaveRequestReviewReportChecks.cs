namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestReviewReportChecks
{
    public static void SeasonalLeaveRequestReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706400243);
        var items = new List<SeasonalLeaveRequestReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview($"seasonal-leave-request-review-{index}", "sample"));
        }
        var report = SeasonalLeaveRequestReviewReport.BuildSeasonalLeaveRequestReviewReport("test", items, clock);
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
        SeasonalLeaveRequestReviewReportCountsEachRecordOnce();
    }
}
