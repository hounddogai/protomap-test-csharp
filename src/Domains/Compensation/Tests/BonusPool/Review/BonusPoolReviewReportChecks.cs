namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolReviewReportChecks
{
    public static void BonusPoolReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704865013);
        var items = new List<BonusPoolReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BonusPoolReview.NewBonusPoolReview($"bonus-pool-review-{index}", "sample"));
        }
        var report = BonusPoolReviewReport.BuildBonusPoolReviewReport("test", items, clock);
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
        BonusPoolReviewReportCountsEachRecordOnce();
    }
}
