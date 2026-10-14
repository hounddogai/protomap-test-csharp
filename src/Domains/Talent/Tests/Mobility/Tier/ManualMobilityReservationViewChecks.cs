namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualMobilityReservationViewChecks
{
    public static void ManualMobilityReservationViewShowsTheStatusInCapitals()
    {
        var item = ManualMobilityReservation.NewManualMobilityReservation("code-634", "title-31");
        var view = ManualMobilityReservationView.ToManualMobilityReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualMobilityReservationView.ToManualMobilityReservationViews(new List<ManualMobilityReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualMobilityReservationViewShowsTheStatusInCapitals();
    }
}
