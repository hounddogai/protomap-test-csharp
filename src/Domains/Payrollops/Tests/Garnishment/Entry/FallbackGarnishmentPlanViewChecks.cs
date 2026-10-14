namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackGarnishmentPlanViewChecks
{
    public static void FallbackGarnishmentPlanViewShowsTheStatusInCapitals()
    {
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
        var view = FallbackGarnishmentPlanView.ToFallbackGarnishmentPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackGarnishmentPlanView.ToFallbackGarnishmentPlanViews(new List<FallbackGarnishmentPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackGarnishmentPlanViewShowsTheStatusInCapitals();
    }
}
