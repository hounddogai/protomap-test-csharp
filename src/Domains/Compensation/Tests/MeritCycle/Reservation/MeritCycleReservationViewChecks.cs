namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleReservationViewChecks
{
    public static void MeritCycleReservationViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleReservation.NewMeritCycleReservation("code-574", "label-41");
        var view = MeritCycleReservationView.ToMeritCycleReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleReservationView.ToMeritCycleReservationViews(new List<MeritCycleReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleReservationViewShowsTheStatusInCapitals();
    }
}
