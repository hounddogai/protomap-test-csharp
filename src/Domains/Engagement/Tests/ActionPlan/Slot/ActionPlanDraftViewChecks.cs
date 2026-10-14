namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftViewChecks
{
    public static void ActionPlanDraftViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanDraft.NewActionPlanDraft("code-335", "label-193");
        var view = ActionPlanDraftView.ToActionPlanDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanDraftView.ToActionPlanDraftViews(new List<ActionPlanDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanDraftViewShowsTheStatusInCapitals();
    }
}
