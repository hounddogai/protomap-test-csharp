namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeReviewReportChecks
{
    public static void AccessBadgeReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703420002);
        var items = new List<AccessBadgeReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AccessBadgeReview.NewAccessBadgeReview($"access-badge-review-{index}", "sample"));
        }
        var report = AccessBadgeReviewReport.BuildAccessBadgeReviewReport("test", items, clock);
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
        AccessBadgeReviewReportCountsEachRecordOnce();
    }
}
