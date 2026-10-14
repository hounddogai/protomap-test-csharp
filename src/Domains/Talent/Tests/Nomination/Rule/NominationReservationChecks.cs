namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationReservationChecks
{
    public static void NominationReservationStartsInProposed()
    {
        var item = NominationReservation.NewNominationReservation("code-762", "label-897");
        Check.Equal(item.Status, NominationReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationReservationMovesToApproved()
    {
        var item = NominationReservation.NewNominationReservation("code-762", "label-897");
        try
        {
            item.MoveTo(NominationReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationReservationStatus.Approved, "status");
    }

    public static void NominationReservationRejectsMovesFromWithdrawn()
    {
        var item = NominationReservation.NewNominationReservation("code-762", "label-897");
        item.Status = NominationReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(NominationReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        NominationReservationStartsInProposed();
        NominationReservationMovesToApproved();
        NominationReservationRejectsMovesFromWithdrawn();
    }
}
