namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorBudgetViewChecks
{
    public static void VendorBudgetViewShowsTheStatusInCapitals()
    {
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
        var view = VendorBudgetView.ToVendorBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorBudgetView.ToVendorBudgetViews(new List<VendorBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorBudgetViewShowsTheStatusInCapitals();
    }
}
