namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryAssetReturnProfileViewChecks
{
    public static void PrimaryAssetReturnProfileViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
        var view = PrimaryAssetReturnProfileView.ToPrimaryAssetReturnProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAssetReturnProfileView.ToPrimaryAssetReturnProfileViews(new List<PrimaryAssetReturnProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAssetReturnProfileViewShowsTheStatusInCapitals();
    }
}
