namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientAdjustmentRollupChecks
{
    public static void LenientAdjustmentRollupStartsInPlanned()
    {
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("code-735", "title-435");
        Check.Equal(item.Status, LenientAdjustmentRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAdjustmentRollupMovesToInProgress()
    {
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("code-735", "title-435");
        try
        {
            item.MoveTo(LenientAdjustmentRollupStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAdjustmentRollupStatus.InProgress, "status");
    }

    public static void LenientAdjustmentRollupRejectsMovesFromCancelled()
    {
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("code-735", "title-435");
        item.Status = LenientAdjustmentRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientAdjustmentRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientAdjustmentRollupStartsInPlanned();
        LenientAdjustmentRollupMovesToInProgress();
        LenientAdjustmentRollupRejectsMovesFromCancelled();
    }
}
