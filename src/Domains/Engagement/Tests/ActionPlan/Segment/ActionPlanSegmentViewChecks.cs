namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanSegmentViewChecks
{
    public static void ActionPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanSegment.NewActionPlanSegment("code-533", "title-767");
        var view = ActionPlanSegmentView.ToActionPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanSegmentView.ToActionPlanSegmentViews(new List<ActionPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanSegmentViewShowsTheStatusInCapitals();
    }
}
