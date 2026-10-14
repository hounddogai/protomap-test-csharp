namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSegment;

using Acme.Workforce.Engagement.ActionPlan.PlanSegment;

/// <summary>Checks of plan segment code, run by the project's runner.</summary>
public static class FallbackActionPlanSegmentViewChecks
{
    public static void FallbackActionPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackActionPlanSegment.NewFallbackActionPlanSegment("code-688", "name-672");
        var view = FallbackActionPlanSegmentView.ToFallbackActionPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackActionPlanSegmentView.ToFallbackActionPlanSegmentViews(new List<FallbackActionPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackActionPlanSegmentViewShowsTheStatusInCapitals();
    }
}
