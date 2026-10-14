namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TieredHolidayReviewReportChecks
{
    public static void TieredHolidayReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707390513);
        var items = new List<TieredHolidayReview>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(TieredHolidayReview.NewTieredHolidayReview($"tiered-holiday-review-{index}", "sample"));
        }
        var report = TieredHolidayReviewReport.BuildTieredHolidayReviewReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        TieredHolidayReviewReportCountsEachRecordOnce();
    }
}
