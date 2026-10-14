namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationAdjustmentChecks
{
    public static void NominationAdjustmentStartsInIdle()
    {
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
        Check.Equal(item.Status, NominationAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationAdjustmentMovesToReady()
    {
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
        try
        {
            item.MoveTo(NominationAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationAdjustmentStatus.Ready, "status");
    }

    public static void NominationAdjustmentRejectsMovesFromStopped()
    {
        var item = NominationAdjustment.NewNominationAdjustment("code-988", "title-208");
        item.Status = NominationAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(NominationAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        NominationAdjustmentStartsInIdle();
        NominationAdjustmentMovesToReady();
        NominationAdjustmentRejectsMovesFromStopped();
    }
}
