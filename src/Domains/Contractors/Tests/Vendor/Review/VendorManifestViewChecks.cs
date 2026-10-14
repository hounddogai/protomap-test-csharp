namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorManifestViewChecks
{
    public static void VendorManifestViewShowsTheStatusInCapitals()
    {
        var item = VendorManifest.NewVendorManifest("code-44", "label-895");
        var view = VendorManifestView.ToVendorManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorManifestView.ToVendorManifestViews(new List<VendorManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorManifestViewShowsTheStatusInCapitals();
    }
}
