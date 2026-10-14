namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupViewChecks
{
    public static void AssetReturnGroupViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
        var view = AssetReturnGroupView.ToAssetReturnGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnGroupView.ToAssetReturnGroupViews(new List<AssetReturnGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnGroupViewShowsTheStatusInCapitals();
    }
}
