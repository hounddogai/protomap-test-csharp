namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanAdjustmentViewChecks
{
    public static void ActionPlanAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanAdjustment.NewActionPlanAdjustment("code-766", "title-665");
        var view = ActionPlanAdjustmentView.ToActionPlanAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanAdjustmentView.ToActionPlanAdjustmentViews(new List<ActionPlanAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanAdjustmentViewShowsTheStatusInCapitals();
    }
}
