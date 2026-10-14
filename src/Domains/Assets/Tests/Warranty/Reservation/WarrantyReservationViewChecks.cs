namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyReservationViewChecks
{
    public static void WarrantyReservationViewShowsTheStatusInCapitals()
    {
        var item = WarrantyReservation.NewWarrantyReservation("code-254", "title-140");
        var view = WarrantyReservationView.ToWarrantyReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyReservationView.ToWarrantyReservationViews(new List<WarrantyReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyReservationViewShowsTheStatusInCapitals();
    }
}
