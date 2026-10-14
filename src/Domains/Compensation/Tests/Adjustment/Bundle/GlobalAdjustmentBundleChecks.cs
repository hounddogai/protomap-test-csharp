namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalAdjustmentBundleChecks
{
    public static void GlobalAdjustmentBundleStartsInNew()
    {
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("code-732", "name-226");
        Check.Equal(item.Status, GlobalAdjustmentBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAdjustmentBundleMovesToQueued()
    {
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("code-732", "name-226");
        try
        {
            item.MoveTo(GlobalAdjustmentBundleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAdjustmentBundleStatus.Queued, "status");
    }

    public static void GlobalAdjustmentBundleRejectsMovesFromFailed()
    {
        var item = GlobalAdjustmentBundle.NewGlobalAdjustmentBundle("code-732", "name-226");
        item.Status = GlobalAdjustmentBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalAdjustmentBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalAdjustmentBundleStartsInNew();
        GlobalAdjustmentBundleMovesToQueued();
        GlobalAdjustmentBundleRejectsMovesFromFailed();
    }
}
