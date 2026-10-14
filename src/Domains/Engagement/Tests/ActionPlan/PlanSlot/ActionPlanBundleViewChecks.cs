namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanBundleViewChecks
{
    public static void ActionPlanBundleViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanBundle.NewActionPlanBundle("code-513", "name-471");
        var view = ActionPlanBundleView.ToActionPlanBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanBundleView.ToActionPlanBundleViews(new List<ActionPlanBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanBundleViewShowsTheStatusInCapitals();
    }
}
