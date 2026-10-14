namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorSegmentViewChecks
{
    public static void DefaultVendorSegmentViewShowsTheStatusInCapitals()
    {
        var item = DefaultVendorSegment.NewDefaultVendorSegment("code-861", "label-968");
        var view = DefaultVendorSegmentView.ToDefaultVendorSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultVendorSegmentView.ToDefaultVendorSegmentViews(new List<DefaultVendorSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultVendorSegmentViewShowsTheStatusInCapitals();
    }
}
