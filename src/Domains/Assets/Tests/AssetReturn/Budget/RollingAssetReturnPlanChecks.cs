namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RollingAssetReturnPlanChecks
{
    public static void RollingAssetReturnPlanStartsInPlanned()
    {
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("code-697", "label-148");
        Check.Equal(item.Status, RollingAssetReturnPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAssetReturnPlanMovesToScheduled()
    {
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("code-697", "label-148");
        try
        {
            item.MoveTo(RollingAssetReturnPlanStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAssetReturnPlanStatus.Scheduled, "status");
    }

    public static void RollingAssetReturnPlanRejectsMovesFromCancelled()
    {
        var item = RollingAssetReturnPlan.NewRollingAssetReturnPlan("code-697", "label-148");
        item.Status = RollingAssetReturnPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingAssetReturnPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingAssetReturnPlanStartsInPlanned();
        RollingAssetReturnPlanMovesToScheduled();
        RollingAssetReturnPlanRejectsMovesFromCancelled();
    }
}
