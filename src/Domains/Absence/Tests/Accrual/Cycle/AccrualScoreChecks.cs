namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualScoreChecks
{
    public static void AccrualScoreStartsInIdle()
    {
        var item = AccrualScore.NewAccrualScore("code-105", "title-755");
        Check.Equal(item.Status, AccrualScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualScoreMovesToDraining()
    {
        var item = AccrualScore.NewAccrualScore("code-105", "title-755");
        try
        {
            item.MoveTo(AccrualScoreStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualScoreStatus.Draining, "status");
    }

    public static void AccrualScoreRejectsMovesFromStopped()
    {
        var item = AccrualScore.NewAccrualScore("code-105", "title-755");
        item.Status = AccrualScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(AccrualScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AccrualScoreStartsInIdle();
        AccrualScoreMovesToDraining();
        AccrualScoreRejectsMovesFromStopped();
    }
}
