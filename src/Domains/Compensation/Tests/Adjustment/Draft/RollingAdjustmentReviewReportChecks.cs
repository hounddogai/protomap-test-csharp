namespace Acme.Workforce.Compensation.Tests.Adjustment.Draft;

using Acme.Workforce.Compensation.Adjustment.Draft;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingAdjustmentReviewReportChecks
{
    public static void RollingAdjustmentReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701088734);
        var items = new List<RollingAdjustmentReview>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RollingAdjustmentReview.NewRollingAdjustmentReview($"rolling-adjustment-review-{index}", "sample"));
        }
        var report = RollingAdjustmentReviewReport.BuildRollingAdjustmentReviewReport("test", items, clock);
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
        RollingAdjustmentReviewReportCountsEachRecordOnce();
    }
}
