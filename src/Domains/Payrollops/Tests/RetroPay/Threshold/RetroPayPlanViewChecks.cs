namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanViewChecks
{
    public static void RetroPayPlanViewShowsTheStatusInCapitals()
    {
        var item = RetroPayPlan.NewRetroPayPlan("code-539", "name-23");
        var view = RetroPayPlanView.ToRetroPayPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayPlanView.ToRetroPayPlanViews(new List<RetroPayPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayPlanViewShowsTheStatusInCapitals();
    }
}
