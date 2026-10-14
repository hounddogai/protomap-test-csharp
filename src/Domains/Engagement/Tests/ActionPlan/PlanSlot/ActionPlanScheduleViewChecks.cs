namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ActionPlanScheduleViewChecks
{
    public static void ActionPlanScheduleViewShowsTheStatusInCapitals()
    {
        var item = ActionPlanSchedule.NewActionPlanSchedule("code-946", "name-141");
        var view = ActionPlanScheduleView.ToActionPlanScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ActionPlanScheduleView.ToActionPlanScheduleViews(new List<ActionPlanSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ActionPlanScheduleViewShowsTheStatusInCapitals();
    }
}
