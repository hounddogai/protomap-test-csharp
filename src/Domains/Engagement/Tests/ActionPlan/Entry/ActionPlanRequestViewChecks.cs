namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestViewChecks
{
    public static void ActionPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanRequest.NewActionPlanRequest("code-526", "name-993");
        var view = ActionPlanRequestView.ToActionPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanRequestView.ToActionPlanRequestViews(new List<ActionPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanRequestViewShowsTheStatusInCapitals();
    }
}
