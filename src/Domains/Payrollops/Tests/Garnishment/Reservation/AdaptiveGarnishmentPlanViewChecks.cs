namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanViewChecks
{
    public static void AdaptiveGarnishmentPlanViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
        var view = AdaptiveGarnishmentPlanView.ToAdaptiveGarnishmentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveGarnishmentPlanView.ToAdaptiveGarnishmentPlanViews(new List<AdaptiveGarnishmentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentPlanViewShowsTheStatusInCapitals();
    }
}
