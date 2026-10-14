namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanRecordViewChecks
{
    public static void ActionPlanRecordViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanRecord.NewActionPlanRecord("code-41", "name-279");
        var view = ActionPlanRecordView.ToActionPlanRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanRecordView.ToActionPlanRecordViews(new List<ActionPlanRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanRecordViewShowsTheStatusInCapitals();
    }
}
