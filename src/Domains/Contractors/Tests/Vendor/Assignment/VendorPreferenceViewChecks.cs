namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VendorPreferenceViewChecks
{
    public static void VendorPreferenceViewShowsTheStatusInCapitals()
    {
        var item = VendorPreference.NewVendorPreference("code-553", "label-302");
        var view = VendorPreferenceView.ToVendorPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorPreferenceView.ToVendorPreferenceViews(new List<VendorPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorPreferenceViewShowsTheStatusInCapitals();
    }
}
