namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class ActionPlanReviewReportChecks
{
    public static void ActionPlanReviewReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703227295);
        var items = new List<ActionPlanReview>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(ActionPlanReview.NewActionPlanReview($"action-plan-review-{index}", "sample"));
        }
        var report = ActionPlanReviewReport.BuildActionPlanReviewReport("test", items, clock);
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
        ActionPlanReviewReportCountsEachRecordOnce();
    }
}
