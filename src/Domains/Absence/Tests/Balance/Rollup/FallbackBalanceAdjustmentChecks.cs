namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackBalanceAdjustmentChecks
{
    public static void FallbackBalanceAdjustmentStartsInIdle()
    {
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("code-901", "label-303");
        Check.Equal(item.Status, FallbackBalanceAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBalanceAdjustmentMovesToWarming()
    {
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("code-901", "label-303");
        try
        {
            item.MoveTo(FallbackBalanceAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBalanceAdjustmentStatus.Warming, "status");
    }

    public static void FallbackBalanceAdjustmentRejectsMovesFromStopped()
    {
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("code-901", "label-303");
        item.Status = FallbackBalanceAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackBalanceAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackBalanceAdjustmentStartsInIdle();
        FallbackBalanceAdjustmentMovesToWarming();
        FallbackBalanceAdjustmentRejectsMovesFromStopped();
    }
}
