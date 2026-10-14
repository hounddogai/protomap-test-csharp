namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class RegionalBalancePolicyChecks
{
    public static void RegionalBalancePolicyStartsInPlanned()
    {
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("code-651", "title-480");
        Check.Equal(item.Status, RegionalBalancePolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBalancePolicyMovesToScheduled()
    {
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("code-651", "title-480");
        try
        {
            item.MoveTo(RegionalBalancePolicyStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBalancePolicyStatus.Scheduled, "status");
    }

    public static void RegionalBalancePolicyRejectsMovesFromCancelled()
    {
        var item = RegionalBalancePolicy.NewRegionalBalancePolicy("code-651", "title-480");
        item.Status = RegionalBalancePolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalBalancePolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalBalancePolicyStartsInPlanned();
        RegionalBalancePolicyMovesToScheduled();
        RegionalBalancePolicyRejectsMovesFromCancelled();
    }
}
