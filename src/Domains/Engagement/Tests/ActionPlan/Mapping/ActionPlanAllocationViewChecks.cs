namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanAllocationViewChecks
{
    public static void ActionPlanAllocationViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
        var view = ActionPlanAllocationView.ToActionPlanAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanAllocationView.ToActionPlanAllocationViews(new List<ActionPlanAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanAllocationViewShowsTheStatusInCapitals();
    }
}
