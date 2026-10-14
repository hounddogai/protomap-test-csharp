namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Checks of pool schedule code, run by the project's runner.</summary>
public static class BonusPoolScoreChecks
{
    public static void BonusPoolScoreStartsInPlanned()
    {
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
        Check.Equal(item.Status, BonusPoolScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolScoreMovesToScheduled()
    {
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
        try
        {
            item.MoveTo(BonusPoolScoreStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolScoreStatus.Scheduled, "status");
    }

    public static void BonusPoolScoreRejectsMovesFromCancelled()
    {
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
        item.Status = BonusPoolScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolScoreStartsInPlanned();
        BonusPoolScoreMovesToScheduled();
        BonusPoolScoreRejectsMovesFromCancelled();
    }
}
