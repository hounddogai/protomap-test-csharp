namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowChecks
{
    public static void CappedNominationWindowStartsInProposed()
    {
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
        Check.Equal(item.Status, CappedNominationWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedNominationWindowMovesToRejected()
    {
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
        try
        {
            item.MoveTo(CappedNominationWindowStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedNominationWindowStatus.Rejected, "status");
    }

    public static void CappedNominationWindowRejectsMovesFromWithdrawn()
    {
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
        item.Status = CappedNominationWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedNominationWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedNominationWindowStartsInProposed();
        CappedNominationWindowMovesToRejected();
        CappedNominationWindowRejectsMovesFromWithdrawn();
    }
}
