namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorDigestViewChecks
{
    public static void VendorDigestViewShowsTheStatusInCapitals()
    {
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
        var view = VendorDigestView.ToVendorDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorDigestView.ToVendorDigestViews(new List<VendorDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorDigestViewShowsTheStatusInCapitals();
    }
}
