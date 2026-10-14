namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PendingAssetReturnPlanViewChecks
{
    public static void PendingAssetReturnPlanViewShowsTheStatusInCapitals()
    {
        var item = PendingAssetReturnPlan.NewPendingAssetReturnPlan("code-531", "label-347");
        var view = PendingAssetReturnPlanView.ToPendingAssetReturnPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAssetReturnPlanView.ToPendingAssetReturnPlanViews(new List<PendingAssetReturnPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAssetReturnPlanViewShowsTheStatusInCapitals();
    }
}
