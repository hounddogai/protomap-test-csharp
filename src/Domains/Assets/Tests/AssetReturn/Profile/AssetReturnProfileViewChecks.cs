namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnProfileViewChecks
{
    public static void AssetReturnProfileViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
        var view = AssetReturnProfileView.ToAssetReturnProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnProfileView.ToAssetReturnProfileViews(new List<AssetReturnProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnProfileViewShowsTheStatusInCapitals();
    }
}
