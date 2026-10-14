namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LegacyBalanceAdjustmentChecks
{
    public static void LegacyBalanceAdjustmentStartsInDraft()
    {
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("code-972", "title-629");
        Check.Equal(item.Status, LegacyBalanceAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyBalanceAdjustmentMovesToPending()
    {
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("code-972", "title-629");
        try
        {
            item.MoveTo(LegacyBalanceAdjustmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyBalanceAdjustmentStatus.Pending, "status");
    }

    public static void LegacyBalanceAdjustmentRejectsMovesFromClosed()
    {
        var item = LegacyBalanceAdjustment.NewLegacyBalanceAdjustment("code-972", "title-629");
        item.Status = LegacyBalanceAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyBalanceAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyBalanceAdjustmentStartsInDraft();
        LegacyBalanceAdjustmentMovesToPending();
        LegacyBalanceAdjustmentRejectsMovesFromClosed();
    }
}
