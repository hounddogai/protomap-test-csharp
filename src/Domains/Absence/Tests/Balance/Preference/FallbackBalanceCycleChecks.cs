namespace Acme.Workforce.Absence.Tests.Balance.Preference;

using Acme.Workforce.Absence.Balance.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackBalanceCycleChecks
{
    public static void FallbackBalanceCycleStartsInProposed()
    {
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("code-430", "label-475");
        Check.Equal(item.Status, FallbackBalanceCycleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBalanceCycleMovesToApproved()
    {
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("code-430", "label-475");
        try
        {
            item.MoveTo(FallbackBalanceCycleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBalanceCycleStatus.Approved, "status");
    }

    public static void FallbackBalanceCycleRejectsMovesFromWithdrawn()
    {
        var item = FallbackBalanceCycle.NewFallbackBalanceCycle("code-430", "label-475");
        item.Status = FallbackBalanceCycleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackBalanceCycleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackBalanceCycleStartsInProposed();
        FallbackBalanceCycleMovesToApproved();
        FallbackBalanceCycleRejectsMovesFromWithdrawn();
    }
}
