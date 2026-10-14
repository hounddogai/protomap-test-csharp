namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class RollingPayRunCycleChecks
{
    public static void RollingPayRunCycleStartsInNew()
    {
        var item = RollingPayRunCycle.NewRollingPayRunCycle("code-57", "label-752");
        Check.Equal(item.Status, RollingPayRunCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPayRunCycleMovesToFailed()
    {
        var item = RollingPayRunCycle.NewRollingPayRunCycle("code-57", "label-752");
        try
        {
            item.MoveTo(RollingPayRunCycleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPayRunCycleStatus.Failed, "status");
    }

    public static void RollingPayRunCycleRejectsMovesFromFailed()
    {
        var item = RollingPayRunCycle.NewRollingPayRunCycle("code-57", "label-752");
        item.Status = RollingPayRunCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingPayRunCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingPayRunCycleStartsInNew();
        RollingPayRunCycleMovesToFailed();
        RollingPayRunCycleRejectsMovesFromFailed();
    }
}
