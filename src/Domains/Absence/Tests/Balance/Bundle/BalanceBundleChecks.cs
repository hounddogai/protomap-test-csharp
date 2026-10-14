namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BalanceBundleChecks
{
    public static void BalanceBundleStartsInPlanned()
    {
        var item = BalanceBundle.NewBalanceBundle("code-167", "label-723");
        Check.Equal(item.Status, BalanceBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceBundleMovesToScheduled()
    {
        var item = BalanceBundle.NewBalanceBundle("code-167", "label-723");
        try
        {
            item.MoveTo(BalanceBundleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceBundleStatus.Scheduled, "status");
    }

    public static void BalanceBundleRejectsMovesFromCancelled()
    {
        var item = BalanceBundle.NewBalanceBundle("code-167", "label-723");
        item.Status = BalanceBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BalanceBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BalanceBundleStartsInPlanned();
        BalanceBundleMovesToScheduled();
        BalanceBundleRejectsMovesFromCancelled();
    }
}
