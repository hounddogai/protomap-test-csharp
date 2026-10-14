namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandReservationViewChecks
{
    public static void DefaultPayBandReservationViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayBandReservation.NewDefaultPayBandReservation("code-140", "title-71");
        var view = DefaultPayBandReservationView.ToDefaultPayBandReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayBandReservationView.ToDefaultPayBandReservationViews(new List<DefaultPayBandReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayBandReservationViewShowsTheStatusInCapitals();
    }
}
