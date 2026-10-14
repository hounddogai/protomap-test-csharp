namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.RetroPay.PayBudget;

/// <summary>Checks of pay budget code, run by the project's runner.</summary>
public static class ManualRetroPayBudgetViewChecks
{
    public static void ManualRetroPayBudgetViewShowsTheStatusInCapitals()
    {
        var item = ManualRetroPayBudget.NewManualRetroPayBudget("code-408", "label-66");
        var view = ManualRetroPayBudgetView.ToManualRetroPayBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualRetroPayBudgetView.ToManualRetroPayBudgetViews(new List<ManualRetroPayBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualRetroPayBudgetViewShowsTheStatusInCapitals();
    }
}
