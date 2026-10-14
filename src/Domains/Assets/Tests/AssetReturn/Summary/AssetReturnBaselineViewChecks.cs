namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssetReturnBaselineViewChecks
{
    public static void AssetReturnBaselineViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnBaseline.NewAssetReturnBaseline("code-701", "title-243");
        var view = AssetReturnBaselineView.ToAssetReturnBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnBaselineView.ToAssetReturnBaselineViews(new List<AssetReturnBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnBaselineViewShowsTheStatusInCapitals();
    }
}
