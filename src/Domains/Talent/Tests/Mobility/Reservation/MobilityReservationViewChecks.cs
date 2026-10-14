namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityReservationViewChecks
{
    public static void MobilityReservationViewShowsTheStatusInCapitals()
    {
        var item = MobilityReservation.NewMobilityReservation("code-842", "label-741");
        var view = MobilityReservationView.ToMobilityReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityReservationView.ToMobilityReservationViews(new List<MobilityReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityReservationViewShowsTheStatusInCapitals();
    }
}
