namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorSegmentViewChecks
{
    public static void VendorSegmentViewShowsTheStatusInCapitals()
    {
        var item = VendorSegment.NewVendorSegment("code-708", "title-369");
        var view = VendorSegmentView.ToVendorSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorSegmentView.ToVendorSegmentViews(new List<VendorSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorSegmentViewShowsTheStatusInCapitals();
    }
}
