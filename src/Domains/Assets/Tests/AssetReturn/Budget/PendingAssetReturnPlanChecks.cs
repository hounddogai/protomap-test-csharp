namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingAssetReturnPlanChecks
{
    public static void PendingAssetReturnPlanStartsInDraft()
    {
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("code-531", "label-347");
        Check.Equal(item.Status, PendingAssetReturnPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAssetReturnPlanMovesToSuspended()
    {
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("code-531", "label-347");
        try
        {
            item.MoveTo(PendingAssetReturnPlanStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAssetReturnPlanStatus.Suspended, "status");
    }

    public static void PendingAssetReturnPlanRejectsMovesFromClosed()
    {
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("code-531", "label-347");
        item.Status = PendingAssetReturnPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingAssetReturnPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingAssetReturnPlanStartsInDraft();
        PendingAssetReturnPlanMovesToSuspended();
        PendingAssetReturnPlanRejectsMovesFromClosed();
    }
}
