namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class WarrantyBudgetViewChecks
{
    public static void WarrantyBudgetViewShowsTheStatusInCapitals()
    {
        var item = WarrantyBudget.NewWarrantyBudget("code-394", "name-262");
        var view = WarrantyBudgetView.ToWarrantyBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyBudgetView.ToWarrantyBudgetViews(new List<WarrantyBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyBudgetViewShowsTheStatusInCapitals();
    }
}
