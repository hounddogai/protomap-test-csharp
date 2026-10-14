namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredAccrualScoreChecks
{
    public static void TieredAccrualScoreStartsInIdle()
    {
        var item = TieredAccrualScore.NewTieredAccrualScore("code-160", "title-640");
        Check.Equal(item.Status, TieredAccrualScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAccrualScoreMovesToWarming()
    {
        var item = TieredAccrualScore.NewTieredAccrualScore("code-160", "title-640");
        try
        {
            item.MoveTo(TieredAccrualScoreStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAccrualScoreStatus.Warming, "status");
    }

    public static void TieredAccrualScoreRejectsMovesFromStopped()
    {
        var item = TieredAccrualScore.NewTieredAccrualScore("code-160", "title-640");
        item.Status = TieredAccrualScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredAccrualScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredAccrualScoreStartsInIdle();
        TieredAccrualScoreMovesToWarming();
        TieredAccrualScoreRejectsMovesFromStopped();
    }
}
