namespace Acme.Workforce.Contractors.Tests.Vendor.VendorRequest;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Checks of vendor request code, run by the project's runner.</summary>
public static class RegionalVendorRequestViewChecks
{
    public static void RegionalVendorRequestViewShowsTheStatusInCapitals()
    {
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
        var view = RegionalVendorRequestView.ToRegionalVendorRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalVendorRequestView.ToRegionalVendorRequestViews(new List<RegionalVendorRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalVendorRequestViewShowsTheStatusInCapitals();
    }
}
