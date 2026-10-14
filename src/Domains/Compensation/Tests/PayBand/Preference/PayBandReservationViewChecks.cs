namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandReservationViewChecks
{
    public static void PayBandReservationViewShowsTheStatusInCapitals()
    {
        var item = PayBandReservation.NewPayBandReservation("code-76", "name-429");
        var view = PayBandReservationView.ToPayBandReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandReservationView.ToPayBandReservationViews(new List<PayBandReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandReservationViewShowsTheStatusInCapitals();
    }
}
