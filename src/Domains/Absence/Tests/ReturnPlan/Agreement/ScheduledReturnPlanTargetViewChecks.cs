namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ScheduledReturnPlanTargetViewChecks
{
    public static void ScheduledReturnPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("code-421", "label-18");
        var view = ScheduledReturnPlanTargetView.ToScheduledReturnPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledReturnPlanTargetView.ToScheduledReturnPlanTargetViews(new List<ScheduledReturnPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledReturnPlanTargetViewShowsTheStatusInCapitals();
    }
}
