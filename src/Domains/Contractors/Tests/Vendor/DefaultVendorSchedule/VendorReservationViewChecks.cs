namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class VendorReservationViewChecks
{
    public static void VendorReservationViewShowsTheStatusInCapitals()
    {
        var item = VendorReservation.NewVendorReservation("code-622", "title-719");
        var view = VendorReservationView.ToVendorReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorReservationView.ToVendorReservationViews(new List<VendorReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorReservationViewShowsTheStatusInCapitals();
    }
}
