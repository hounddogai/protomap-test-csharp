namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryViewChecks
{
    public static void ActionPlanEntryViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
        var view = ActionPlanEntryView.ToActionPlanEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanEntryView.ToActionPlanEntryViews(new List<ActionPlanEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanEntryViewShowsTheStatusInCapitals();
    }
}
