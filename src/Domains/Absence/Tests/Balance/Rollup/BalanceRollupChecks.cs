namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BalanceRollupChecks
{
    public static void BalanceRollupStartsInNew()
    {
        var item = BalanceRollup.NewBalanceRollup("code-201", "name-206");
        Check.Equal(item.Status, BalanceRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceRollupMovesToFailed()
    {
        var item = BalanceRollup.NewBalanceRollup("code-201", "name-206");
        try
        {
            item.MoveTo(BalanceRollupStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceRollupStatus.Failed, "status");
    }

    public static void BalanceRollupRejectsMovesFromFailed()
    {
        var item = BalanceRollup.NewBalanceRollup("code-201", "name-206");
        item.Status = BalanceRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(BalanceRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        BalanceRollupStartsInNew();
        BalanceRollupMovesToFailed();
        BalanceRollupRejectsMovesFromFailed();
    }
}
