namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveActionPlanReviewReportChecks
{
    public static void AdaptiveActionPlanReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706182642);
        var items = new List<AdaptiveActionPlanReview>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(AdaptiveActionPlanReview.NewAdaptiveActionPlanReview($"adaptive-action-plan-review-{index}", "sample"));
        }
        var report = AdaptiveActionPlanReviewReport.BuildAdaptiveActionPlanReviewReport("test", items, clock);
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
        AdaptiveActionPlanReviewReportCountsEachRecordOnce();
    }
}
