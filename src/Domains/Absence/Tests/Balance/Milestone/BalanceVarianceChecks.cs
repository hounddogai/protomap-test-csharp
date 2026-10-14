namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceChecks
{
    public static void BalanceVarianceStartsInProposed()
    {
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
        Check.Equal(item.Status, BalanceVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceVarianceMovesToRejected()
    {
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
        try
        {
            item.MoveTo(BalanceVarianceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceVarianceStatus.Rejected, "status");
    }

    public static void BalanceVarianceRejectsMovesFromWithdrawn()
    {
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
        item.Status = BalanceVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BalanceVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BalanceVarianceStartsInProposed();
        BalanceVarianceMovesToRejected();
        BalanceVarianceRejectsMovesFromWithdrawn();
    }
}
