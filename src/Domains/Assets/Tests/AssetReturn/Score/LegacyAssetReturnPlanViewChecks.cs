namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LegacyAssetReturnPlanViewChecks
{
    public static void LegacyAssetReturnPlanViewShowsTheStatusInCapitals()
    {
        var item = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan("code-983", "title-160");
        var view = LegacyAssetReturnPlanView.ToLegacyAssetReturnPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAssetReturnPlanView.ToLegacyAssetReturnPlanViews(new List<LegacyAssetReturnPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAssetReturnPlanViewShowsTheStatusInCapitals();
    }
}
