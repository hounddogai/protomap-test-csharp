namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnVarianceViewChecks
{
    public static void AdaptiveAssetReturnVarianceViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance("code-870", "name-802");
        var view = AdaptiveAssetReturnVarianceView.ToAdaptiveAssetReturnVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAssetReturnVarianceView.ToAdaptiveAssetReturnVarianceViews(new List<AdaptiveAssetReturnVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnVarianceViewShowsTheStatusInCapitals();
    }
}
