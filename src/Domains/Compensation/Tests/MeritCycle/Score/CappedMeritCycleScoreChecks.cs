namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CappedMeritCycleScoreChecks
{
    public static void CappedMeritCycleScoreStartsInRequested()
    {
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("code-465", "title-529");
        Check.Equal(item.Status, CappedMeritCycleScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedMeritCycleScoreMovesToRevoked()
    {
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("code-465", "title-529");
        try
        {
            item.MoveTo(CappedMeritCycleScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedMeritCycleScoreStatus.Revoked, "status");
    }

    public static void CappedMeritCycleScoreRejectsMovesFromExpired()
    {
        var item = CappedMeritCycleScore.NewCappedMeritCycleScore("code-465", "title-529");
        item.Status = CappedMeritCycleScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedMeritCycleScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedMeritCycleScoreStartsInRequested();
        CappedMeritCycleScoreMovesToRevoked();
        CappedMeritCycleScoreRejectsMovesFromExpired();
    }
}
