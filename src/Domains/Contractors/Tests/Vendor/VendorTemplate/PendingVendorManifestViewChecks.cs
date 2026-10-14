namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class PendingVendorManifestViewChecks
{
    public static void PendingVendorManifestViewShowsTheStatusInCapitals()
    {
        var item = PendingVendorManifest.NewPendingVendorManifest("code-304", "label-138");
        var view = PendingVendorManifestView.ToPendingVendorManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVendorManifestView.ToPendingVendorManifestViews(new List<PendingVendorManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVendorManifestViewShowsTheStatusInCapitals();
    }
}
