namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceOverrideChecks
{
    public static void BalanceOverrideStartsInNew()
    {
        var item = BalanceOverride.NewBalanceOverride("code-757", "name-939");
        Check.Equal(item.Status, BalanceOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceOverrideMovesToFailed()
    {
        var item = BalanceOverride.NewBalanceOverride("code-757", "name-939");
        try
        {
            item.MoveTo(BalanceOverrideStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceOverrideStatus.Failed, "status");
    }

    public static void BalanceOverrideRejectsMovesFromFailed()
    {
        var item = BalanceOverride.NewBalanceOverride("code-757", "name-939");
        item.Status = BalanceOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(BalanceOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        BalanceOverrideStartsInNew();
        BalanceOverrideMovesToFailed();
        BalanceOverrideRejectsMovesFromFailed();
    }
}
