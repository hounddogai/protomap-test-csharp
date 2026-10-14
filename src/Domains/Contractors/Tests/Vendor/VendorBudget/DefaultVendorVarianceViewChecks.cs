namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBudget;

using Acme.Workforce.Contractors.Vendor.VendorBudget;

/// <summary>Checks of vendor budget code, run by the project's runner.</summary>
public static class DefaultVendorVarianceViewChecks
{
    public static void DefaultVendorVarianceViewShowsTheStatusInCapitals()
    {
        var item = DefaultVendorVariance.NewDefaultVendorVariance("code-742", "title-510");
        var view = DefaultVendorVarianceView.ToDefaultVendorVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVendorVarianceView.ToDefaultVendorVarianceViews(new List<DefaultVendorVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVendorVarianceViewShowsTheStatusInCapitals();
    }
}
