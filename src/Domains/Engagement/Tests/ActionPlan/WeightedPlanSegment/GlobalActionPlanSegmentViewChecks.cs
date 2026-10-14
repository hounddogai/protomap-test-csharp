namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanSegmentViewChecks
{
    public static void GlobalActionPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = GlobalActionPlanSegment.NewGlobalActionPlanSegment("code-818", "name-561");
        var view = GlobalActionPlanSegmentView.ToGlobalActionPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalActionPlanSegmentView.ToGlobalActionPlanSegmentViews(new List<GlobalActionPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalActionPlanSegmentViewShowsTheStatusInCapitals();
    }
}
