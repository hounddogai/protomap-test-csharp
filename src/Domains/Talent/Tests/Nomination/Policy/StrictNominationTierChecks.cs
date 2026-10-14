namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class StrictNominationTierChecks
{
    public static void StrictNominationTierStartsInNew()
    {
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
        Check.Equal(item.Status, StrictNominationTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictNominationTierMovesToRunning()
    {
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
        try
        {
            item.MoveTo(StrictNominationTierStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictNominationTierStatus.Running, "status");
    }

    public static void StrictNominationTierRejectsMovesFromFailed()
    {
        var item = StrictNominationTier.NewStrictNominationTier("code-302", "title-961");
        item.Status = StrictNominationTierStatus.Failed;
        Check.Fails(() => item.MoveTo(StrictNominationTierStatus.New), "move");
    }

    public static void RunAll()
    {
        StrictNominationTierStartsInNew();
        StrictNominationTierMovesToRunning();
        StrictNominationTierRejectsMovesFromFailed();
    }
}
