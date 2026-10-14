namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>Checks of grant metric code, run by the project's runner.</summary>
public static class EquityGrantScoreChecks
{
    public static void EquityGrantScoreStartsInNew()
    {
        var item = EquityGrantScore.NewEquityGrantScore("code-671", "name-682");
        Check.Equal(item.Status, EquityGrantScoreStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantScoreMovesToRunning()
    {
        var item = EquityGrantScore.NewEquityGrantScore("code-671", "name-682");
        try
        {
            item.MoveTo(EquityGrantScoreStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantScoreStatus.Running, "status");
    }

    public static void EquityGrantScoreRejectsMovesFromFailed()
    {
        var item = EquityGrantScore.NewEquityGrantScore("code-671", "name-682");
        item.Status = EquityGrantScoreStatus.Failed;
        Check.Fails(() => item.MoveTo(EquityGrantScoreStatus.New), "move");
    }

    public static void RunAll()
    {
        EquityGrantScoreStartsInNew();
        EquityGrantScoreMovesToRunning();
        EquityGrantScoreRejectsMovesFromFailed();
    }
}
