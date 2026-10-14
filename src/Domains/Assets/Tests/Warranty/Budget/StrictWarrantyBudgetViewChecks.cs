namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictWarrantyBudgetViewChecks
{
    public static void StrictWarrantyBudgetViewShowsTheStatusInCapitals()
    {
        var item = StrictWarrantyBudget.NewStrictWarrantyBudget("code-182", "title-64");
        var view = StrictWarrantyBudgetView.ToStrictWarrantyBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictWarrantyBudgetView.ToStrictWarrantyBudgetViews(new List<StrictWarrantyBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictWarrantyBudgetViewShowsTheStatusInCapitals();
    }
}
