namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class WeightedGarnishmentBudgetViewChecks
{
    public static void WeightedGarnishmentBudgetViewShowsTheStatusInCapitals()
    {
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("code-706", "name-711");
        var view = WeightedGarnishmentBudgetView.ToWeightedGarnishmentBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedGarnishmentBudgetView.ToWeightedGarnishmentBudgetViews(new List<WeightedGarnishmentBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedGarnishmentBudgetViewShowsTheStatusInCapitals();
    }
}
