namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalancePolicyChecks
{
    public static void BalancePolicyStartsInDraft()
    {
        var item = BalancePolicy.NewBalancePolicy("code-725", "name-875");
        Check.Equal(item.Status, BalancePolicyStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalancePolicyMovesToActive()
    {
        var item = BalancePolicy.NewBalancePolicy("code-725", "name-875");
        try
        {
            item.MoveTo(BalancePolicyStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalancePolicyStatus.Active, "status");
    }

    public static void BalancePolicyRejectsMovesFromClosed()
    {
        var item = BalancePolicy.NewBalancePolicy("code-725", "name-875");
        item.Status = BalancePolicyStatus.Closed;
        Check.Fails(() => item.MoveTo(BalancePolicyStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BalancePolicyStartsInDraft();
        BalancePolicyMovesToActive();
        BalancePolicyRejectsMovesFromClosed();
    }
}
