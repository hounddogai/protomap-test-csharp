namespace Acme.Workforce.Engagement.Tests.ActionPlan.Segment;

using Acme.Workforce.Engagement.ActionPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ActionPlanMilestoneViewChecks
{
    public static void ActionPlanMilestoneViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanMilestone.NewActionPlanMilestone("code-186", "label-774");
        var view = ActionPlanMilestoneView.ToActionPlanMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanMilestoneView.ToActionPlanMilestoneViews(new List<ActionPlanMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanMilestoneViewShowsTheStatusInCapitals();
    }
}
