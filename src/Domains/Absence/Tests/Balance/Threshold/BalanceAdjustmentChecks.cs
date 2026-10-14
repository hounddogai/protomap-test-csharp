namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BalanceAdjustmentChecks
{
    public static void BalanceAdjustmentStartsInIdle()
    {
        var item = BalanceAdjustment.NewBalanceAdjustment("code-491", "title-863");
        Check.Equal(item.Status, BalanceAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceAdjustmentMovesToWarming()
    {
        var item = BalanceAdjustment.NewBalanceAdjustment("code-491", "title-863");
        try
        {
            item.MoveTo(BalanceAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceAdjustmentStatus.Warming, "status");
    }

    public static void BalanceAdjustmentRejectsMovesFromStopped()
    {
        var item = BalanceAdjustment.NewBalanceAdjustment("code-491", "title-863");
        item.Status = BalanceAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(BalanceAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BalanceAdjustmentStartsInIdle();
        BalanceAdjustmentMovesToWarming();
        BalanceAdjustmentRejectsMovesFromStopped();
    }
}
