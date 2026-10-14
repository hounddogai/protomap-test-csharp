namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationTierChecks
{
    public static void NominationTierStartsInNew()
    {
        var item = NominationTier.NewNominationTier("code-837", "label-532");
        Check.Equal(item.Status, NominationTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationTierMovesToDone()
    {
        var item = NominationTier.NewNominationTier("code-837", "label-532");
        try
        {
            item.MoveTo(NominationTierStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationTierStatus.Done, "status");
    }

    public static void NominationTierRejectsMovesFromFailed()
    {
        var item = NominationTier.NewNominationTier("code-837", "label-532");
        item.Status = NominationTierStatus.Failed;
        Check.Fails(() => item.MoveTo(NominationTierStatus.New), "move");
    }

    public static void RunAll()
    {
        NominationTierStartsInNew();
        NominationTierMovesToDone();
        NominationTierRejectsMovesFromFailed();
    }
}
