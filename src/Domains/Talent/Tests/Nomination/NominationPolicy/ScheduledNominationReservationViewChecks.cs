namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationReservationViewChecks
{
    public static void ScheduledNominationReservationViewShowsTheStatusInCapitals()
    {
        var item = ScheduledNominationReservation.NewScheduledNominationReservation("code-669", "title-600");
        var view = ScheduledNominationReservationView.ToScheduledNominationReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledNominationReservationView.ToScheduledNominationReservationViews(new List<ScheduledNominationReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledNominationReservationViewShowsTheStatusInCapitals();
    }
}
