namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class ScheduledNominationReservationChecks
{
    public static void ScheduledNominationReservationStartsInProposed()
    {
        var item = ScheduledNominationReservation.NewScheduledNominationReservation("code-669", "title-600");
        Check.Equal(item.Status, ScheduledNominationReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledNominationReservationMovesToApproved()
    {
        var item = ScheduledNominationReservation.NewScheduledNominationReservation("code-669", "title-600");
        try
        {
            item.MoveTo(ScheduledNominationReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledNominationReservationStatus.Approved, "status");
    }

    public static void ScheduledNominationReservationRejectsMovesFromWithdrawn()
    {
        var item = ScheduledNominationReservation.NewScheduledNominationReservation("code-669", "title-600");
        item.Status = ScheduledNominationReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledNominationReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledNominationReservationStartsInProposed();
        ScheduledNominationReservationMovesToApproved();
        ScheduledNominationReservationRejectsMovesFromWithdrawn();
    }
}
