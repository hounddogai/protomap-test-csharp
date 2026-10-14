namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyNominationReservationViewChecks
{
    public static void LegacyNominationReservationViewShowsTheStatusInCapitals()
    {
        var item = LegacyNominationReservation.NewLegacyNominationReservation("code-708", "label-899");
        var view = LegacyNominationReservationView.ToLegacyNominationReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyNominationReservationView.ToLegacyNominationReservationViews(new List<LegacyNominationReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyNominationReservationViewShowsTheStatusInCapitals();
    }
}
