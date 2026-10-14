namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationViewChecks
{
    public static void CappedRetroPayReservationViewShowsTheStatusInCapitals()
    {
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
        var view = CappedRetroPayReservationView.ToCappedRetroPayReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedRetroPayReservationView.ToCappedRetroPayReservationViews(new List<CappedRetroPayReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedRetroPayReservationViewShowsTheStatusInCapitals();
    }
}
