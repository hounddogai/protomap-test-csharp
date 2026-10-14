namespace Acme.Workforce.Contractors.Tests.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Checks of scheduled vendor schedule code, run by the project's runner.</summary>
public static class VendorProfileViewChecks
{
    public static void VendorProfileViewShowsTheStatusInCapitals()
    {
        var item = VendorProfile.NewVendorProfile("code-66", "label-233");
        var view = VendorProfileView.ToVendorProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorProfileView.ToVendorProfileViews(new List<VendorProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorProfileViewShowsTheStatusInCapitals();
    }
}
