namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedVendorEntryViewChecks
{
    public static void CappedVendorEntryViewShowsTheStatusInCapitals()
    {
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
        var view = CappedVendorEntryView.ToCappedVendorEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedVendorEntryView.ToCappedVendorEntryViews(new List<CappedVendorEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedVendorEntryViewShowsTheStatusInCapitals();
    }
}
