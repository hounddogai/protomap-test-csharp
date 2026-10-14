namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledActionPlanAdjustmentViewChecks
{
    public static void ScheduledActionPlanAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("code-485", "name-746");
        var view = ScheduledActionPlanAdjustmentView.ToScheduledActionPlanAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledActionPlanAdjustmentView.ToScheduledActionPlanAdjustmentViews(new List<ScheduledActionPlanAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledActionPlanAdjustmentViewShowsTheStatusInCapitals();
    }
}
