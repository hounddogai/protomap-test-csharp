namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleReservationViewChecks
{
    public static void AdaptiveMeritCycleReservationViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("code-287", "title-79");
        var view = AdaptiveMeritCycleReservationView.ToAdaptiveMeritCycleReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveMeritCycleReservationView.ToAdaptiveMeritCycleReservationViews(new List<AdaptiveMeritCycleReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleReservationViewShowsTheStatusInCapitals();
    }
}
