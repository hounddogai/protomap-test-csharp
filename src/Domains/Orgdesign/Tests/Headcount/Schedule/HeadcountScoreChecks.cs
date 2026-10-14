namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountScoreChecks
{
    public static void HeadcountScoreStartsInRequested()
    {
        var item = HeadcountScore.NewHeadcountScore("code-598", "title-65");
        Check.Equal(item.Status, HeadcountScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountScoreMovesToRevoked()
    {
        var item = HeadcountScore.NewHeadcountScore("code-598", "title-65");
        try
        {
            item.MoveTo(HeadcountScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountScoreStatus.Revoked, "status");
    }

    public static void HeadcountScoreRejectsMovesFromExpired()
    {
        var item = HeadcountScore.NewHeadcountScore("code-598", "title-65");
        item.Status = HeadcountScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(HeadcountScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HeadcountScoreStartsInRequested();
        HeadcountScoreMovesToRevoked();
        HeadcountScoreRejectsMovesFromExpired();
    }
}
