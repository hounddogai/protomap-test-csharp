namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LegacyRetroPayPlanViewChecks
{
    public static void LegacyRetroPayPlanViewShowsTheStatusInCapitals()
    {
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("code-11", "name-743");
        var view = LegacyRetroPayPlanView.ToLegacyRetroPayPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyRetroPayPlanView.ToLegacyRetroPayPlanViews(new List<LegacyRetroPayPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyRetroPayPlanViewShowsTheStatusInCapitals();
    }
}
