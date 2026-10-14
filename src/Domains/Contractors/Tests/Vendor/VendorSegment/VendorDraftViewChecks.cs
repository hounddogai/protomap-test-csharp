namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorDraftViewChecks
{
    public static void VendorDraftViewShowsTheStatusInCapitals()
    {
        var item = VendorDraft.NewVendorDraft("code-548", "title-748");
        var view = VendorDraftView.ToVendorDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorDraftView.ToVendorDraftViews(new List<VendorDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorDraftViewShowsTheStatusInCapitals();
    }
}
