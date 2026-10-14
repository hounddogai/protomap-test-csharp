namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorWindowViewChecks
{
    public static void VendorWindowViewShowsTheStatusInCapitals()
    {
        var item = VendorWindow.NewVendorWindow("code-56", "label-667");
        var view = VendorWindowView.ToVendorWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorWindowView.ToVendorWindowViews(new List<VendorWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorWindowViewShowsTheStatusInCapitals();
    }
}
