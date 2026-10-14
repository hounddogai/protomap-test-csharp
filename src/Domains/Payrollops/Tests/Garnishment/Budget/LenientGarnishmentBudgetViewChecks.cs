namespace Acme.Workforce.Payrollops.Tests.Garnishment.Budget;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientGarnishmentBudgetViewChecks
{
    public static void LenientGarnishmentBudgetViewShowsTheStatusInCapitals()
    {
        var item = LenientGarnishmentBudget.NewLenientGarnishmentBudget("code-250", "label-403");
        var view = LenientGarnishmentBudgetView.ToLenientGarnishmentBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientGarnishmentBudgetView.ToLenientGarnishmentBudgetViews(new List<LenientGarnishmentBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientGarnishmentBudgetViewShowsTheStatusInCapitals();
    }
}
