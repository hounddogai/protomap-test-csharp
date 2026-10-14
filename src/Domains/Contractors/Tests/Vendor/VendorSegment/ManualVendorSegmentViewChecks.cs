namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class ManualVendorSegmentViewChecks
{
    public static void ManualVendorSegmentViewShowsTheStatusInCapitals()
    {
        var item = ManualVendorSegment.NewManualVendorSegment("code-775", "title-238");
        var view = ManualVendorSegmentView.ToManualVendorSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualVendorSegmentView.ToManualVendorSegmentViews(new List<ManualVendorSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualVendorSegmentViewShowsTheStatusInCapitals();
    }
}
