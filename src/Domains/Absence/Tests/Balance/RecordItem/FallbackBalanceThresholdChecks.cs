namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FallbackBalanceThresholdChecks
{
    public static void FallbackBalanceThresholdStartsInProposed()
    {
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("code-680", "title-535");
        Check.Equal(item.Status, FallbackBalanceThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBalanceThresholdMovesToWithdrawn()
    {
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("code-680", "title-535");
        try
        {
            item.MoveTo(FallbackBalanceThresholdStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBalanceThresholdStatus.Withdrawn, "status");
    }

    public static void FallbackBalanceThresholdRejectsMovesFromWithdrawn()
    {
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("code-680", "title-535");
        item.Status = FallbackBalanceThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackBalanceThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackBalanceThresholdStartsInProposed();
        FallbackBalanceThresholdMovesToWithdrawn();
        FallbackBalanceThresholdRejectsMovesFromWithdrawn();
    }
}
