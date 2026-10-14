namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class RegionalBalanceTargetChecks
{
    public static void RegionalBalanceTargetStartsInProposed()
    {
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("code-502", "name-176");
        Check.Equal(item.Status, RegionalBalanceTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBalanceTargetMovesToRejected()
    {
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("code-502", "name-176");
        try
        {
            item.MoveTo(RegionalBalanceTargetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBalanceTargetStatus.Rejected, "status");
    }

    public static void RegionalBalanceTargetRejectsMovesFromWithdrawn()
    {
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("code-502", "name-176");
        item.Status = RegionalBalanceTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalBalanceTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalBalanceTargetStartsInProposed();
        RegionalBalanceTargetMovesToRejected();
        RegionalBalanceTargetRejectsMovesFromWithdrawn();
    }
}
