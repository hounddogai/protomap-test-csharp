namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictNominationScoreChecks
{
    public static void StrictNominationScoreStartsInCreated()
    {
        var item = StrictNominationScore.NewStrictNominationScore("code-880", "name-318");
        Check.Equal(item.Status, StrictNominationScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictNominationScoreMovesToValidated()
    {
        var item = StrictNominationScore.NewStrictNominationScore("code-880", "name-318");
        try
        {
            item.MoveTo(StrictNominationScoreStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictNominationScoreStatus.Validated, "status");
    }

    public static void StrictNominationScoreRejectsMovesFromRetired()
    {
        var item = StrictNominationScore.NewStrictNominationScore("code-880", "name-318");
        item.Status = StrictNominationScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictNominationScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictNominationScoreStartsInCreated();
        StrictNominationScoreMovesToValidated();
        StrictNominationScoreRejectsMovesFromRetired();
    }
}
