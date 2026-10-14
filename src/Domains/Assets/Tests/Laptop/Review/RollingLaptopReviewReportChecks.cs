namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingLaptopReviewReportChecks
{
    public static void RollingLaptopReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700723618);
        var items = new List<RollingLaptopReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(RollingLaptopReview.NewRollingLaptopReview($"rolling-laptop-review-{index}", "sample"));
        }
        var report = RollingLaptopReviewReport.BuildRollingLaptopReviewReport("test", items, clock);
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
        RollingLaptopReviewReportCountsEachRecordOnce();
    }
}
