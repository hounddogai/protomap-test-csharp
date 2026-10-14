namespace Acme.Workforce.Talent.Tests.Nomination.Window;

using Acme.Workforce.Talent.Nomination.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class NominationScoreChecks
{
    public static void NominationScoreStartsInOpen()
    {
        var item = NominationScore.NewNominationScore("code-546", "label-366");
        Check.Equal(item.Status, NominationScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationScoreMovesToReopened()
    {
        var item = NominationScore.NewNominationScore("code-546", "label-366");
        try
        {
            item.MoveTo(NominationScoreStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationScoreStatus.Reopened, "status");
    }

    public static void NominationScoreRejectsMovesFromArchived()
    {
        var item = NominationScore.NewNominationScore("code-546", "label-366");
        item.Status = NominationScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(NominationScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        NominationScoreStartsInOpen();
        NominationScoreMovesToReopened();
        NominationScoreRejectsMovesFromArchived();
    }
}
