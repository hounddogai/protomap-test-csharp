namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledGarnishmentPlanViewChecks
{
    public static void ScheduledGarnishmentPlanViewShowsTheStatusInCapitals()
    {
        var item = ScheduledGarnishmentPlan.NewScheduledGarnishmentPlan("code-702", "title-667");
        var view = ScheduledGarnishmentPlanView.ToScheduledGarnishmentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledGarnishmentPlanView.ToScheduledGarnishmentPlanViews(new List<ScheduledGarnishmentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledGarnishmentPlanViewShowsTheStatusInCapitals();
    }
}
