namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ActionPlanGroupViewChecks
{
    public static void ActionPlanGroupViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanGroup.NewActionPlanGroup("code-415", "label-523");
        var view = ActionPlanGroupView.ToActionPlanGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanGroupView.ToActionPlanGroupViews(new List<ActionPlanGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanGroupViewShowsTheStatusInCapitals();
    }
}
