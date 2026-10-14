namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyNominationReservationChecks
{
    public static void LegacyNominationReservationStartsInIdle()
    {
        var item = LegacyNominationReservation.NewLegacyNominationReservation("code-708", "label-899");
        Check.Equal(item.Status, LegacyNominationReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyNominationReservationMovesToReady()
    {
        var item = LegacyNominationReservation.NewLegacyNominationReservation("code-708", "label-899");
        try
        {
            item.MoveTo(LegacyNominationReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyNominationReservationStatus.Ready, "status");
    }

    public static void LegacyNominationReservationRejectsMovesFromStopped()
    {
        var item = LegacyNominationReservation.NewLegacyNominationReservation("code-708", "label-899");
        item.Status = LegacyNominationReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyNominationReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyNominationReservationStartsInIdle();
        LegacyNominationReservationMovesToReady();
        LegacyNominationReservationRejectsMovesFromStopped();
    }
}
