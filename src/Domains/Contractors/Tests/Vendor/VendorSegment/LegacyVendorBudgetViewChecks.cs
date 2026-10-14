namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class LegacyVendorBudgetViewChecks
{
    public static void LegacyVendorBudgetViewShowsTheStatusInCapitals()
    {
        var item = LegacyVendorBudget.NewLegacyVendorBudget("code-198", "name-583");
        var view = LegacyVendorBudgetView.ToLegacyVendorBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyVendorBudgetView.ToLegacyVendorBudgetViews(new List<LegacyVendorBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyVendorBudgetViewShowsTheStatusInCapitals();
    }
}
