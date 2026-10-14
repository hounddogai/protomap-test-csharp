namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RollingPoolLedgerChecks
{
    public static void RollingPoolLedgerStartsInNew()
    {
        var item = RollingPoolLedger.NewRollingPoolLedger("code-806", "label-590");
        Check.Equal(item.Status, RollingPoolLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPoolLedgerMovesToRunning()
    {
        var item = RollingPoolLedger.NewRollingPoolLedger("code-806", "label-590");
        try
        {
            item.MoveTo(RollingPoolLedgerStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPoolLedgerStatus.Running, "status");
    }

    public static void RollingPoolLedgerRejectsMovesFromFailed()
    {
        var item = RollingPoolLedger.NewRollingPoolLedger("code-806", "label-590");
        item.Status = RollingPoolLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPoolLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPoolLedgerStartsInNew();
        RollingPoolLedgerMovesToRunning();
        RollingPoolLedgerRejectsMovesFromFailed();
    }
}
