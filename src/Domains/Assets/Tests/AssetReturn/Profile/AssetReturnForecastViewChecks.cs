namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnForecastViewChecks
{
    public static void AssetReturnForecastViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnForecast.NewAssetReturnForecast("code-795", "name-694");
        var view = AssetReturnForecastView.ToAssetReturnForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnForecastView.ToAssetReturnForecastViews(new List<AssetReturnForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnForecastViewShowsTheStatusInCapitals();
    }
}
