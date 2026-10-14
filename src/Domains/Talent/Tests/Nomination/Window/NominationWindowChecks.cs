namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationWindowChecks
{
    public static void NominationWindowStartsInProposed()
    {
        var item = NominationWindow.NewNominationWindow("code-540", "label-887");
        Check.Equal(item.Status, NominationWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationWindowMovesToRejected()
    {
        var item = NominationWindow.NewNominationWindow("code-540", "label-887");
        try
        {
            item.MoveTo(NominationWindowStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationWindowStatus.Rejected, "status");
    }

    public static void NominationWindowRejectsMovesFromWithdrawn()
    {
        var item = NominationWindow.NewNominationWindow("code-540", "label-887");
        item.Status = NominationWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(NominationWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        NominationWindowStartsInProposed();
        NominationWindowMovesToRejected();
        NominationWindowRejectsMovesFromWithdrawn();
    }
}
