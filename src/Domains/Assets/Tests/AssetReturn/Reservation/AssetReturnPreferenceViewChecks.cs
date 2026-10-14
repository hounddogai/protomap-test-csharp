namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnPreferenceViewChecks
{
    public static void AssetReturnPreferenceViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnPreference.NewAssetReturnPreference("code-672", "label-276");
        var view = AssetReturnPreferenceView.ToAssetReturnPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnPreferenceView.ToAssetReturnPreferenceViews(new List<AssetReturnPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnPreferenceViewShowsTheStatusInCapitals();
    }
}
