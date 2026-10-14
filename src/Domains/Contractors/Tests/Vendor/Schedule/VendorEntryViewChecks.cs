namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VendorEntryViewChecks
{
    public static void VendorEntryViewShowsTheStatusInCapitals()
    {
        var item = VendorEntry.NewVendorEntry("code-70", "title-43");
        var view = VendorEntryView.ToVendorEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorEntryView.ToVendorEntryViews(new List<VendorEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorEntryViewShowsTheStatusInCapitals();
    }
}
