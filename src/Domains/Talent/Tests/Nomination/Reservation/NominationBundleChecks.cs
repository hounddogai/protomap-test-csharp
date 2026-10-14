namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class NominationBundleChecks
{
    public static void NominationBundleStartsInDraft()
    {
        var item = NominationBundle.NewNominationBundle("code-261", "name-279");
        Check.Equal(item.Status, NominationBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationBundleMovesToPending()
    {
        var item = NominationBundle.NewNominationBundle("code-261", "name-279");
        try
        {
            item.MoveTo(NominationBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationBundleStatus.Pending, "status");
    }

    public static void NominationBundleRejectsMovesFromClosed()
    {
        var item = NominationBundle.NewNominationBundle("code-261", "name-279");
        item.Status = NominationBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(NominationBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        NominationBundleStartsInDraft();
        NominationBundleMovesToPending();
        NominationBundleRejectsMovesFromClosed();
    }
}
