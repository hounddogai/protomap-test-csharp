namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingViewChecks
{
    public static void VendorMappingViewShowsTheStatusInCapitals()
    {
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
        var view = VendorMappingView.ToVendorMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorMappingView.ToVendorMappingViews(new List<VendorMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorMappingViewShowsTheStatusInCapitals();
    }
}
