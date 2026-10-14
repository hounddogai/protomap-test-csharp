namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnScoreViewChecks
{
    public static void AssetReturnScoreViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnScore.NewAssetReturnScore("code-986", "name-337");
        var view = AssetReturnScoreView.ToAssetReturnScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnScoreView.ToAssetReturnScoreViews(new List<AssetReturnScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnScoreViewShowsTheStatusInCapitals();
    }
}
