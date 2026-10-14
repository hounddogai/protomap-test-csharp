namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupChecks
{
    public static void GlobalAdjustmentRollupStartsInPlanned()
    {
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
        Check.Equal(item.Status, GlobalAdjustmentRollupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAdjustmentRollupMovesToCancelled()
    {
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
        try
        {
            item.MoveTo(GlobalAdjustmentRollupStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAdjustmentRollupStatus.Cancelled, "status");
    }

    public static void GlobalAdjustmentRollupRejectsMovesFromCancelled()
    {
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
        item.Status = GlobalAdjustmentRollupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalAdjustmentRollupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRollupStartsInPlanned();
        GlobalAdjustmentRollupMovesToCancelled();
        GlobalAdjustmentRollupRejectsMovesFromCancelled();
    }
}
