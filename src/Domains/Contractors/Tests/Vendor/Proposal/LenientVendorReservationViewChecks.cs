namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationViewChecks
{
    public static void LenientVendorReservationViewShowsTheStatusInCapitals()
    {
        var item = LenientVendorReservation.NewLenientVendorReservation("code-153", "name-378");
        var view = LenientVendorReservationView.ToLenientVendorReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientVendorReservationView.ToLenientVendorReservationViews(new List<LenientVendorReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientVendorReservationViewShowsTheStatusInCapitals();
    }
}
