namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackAssetReturnPreferenceViewChecks
{
    public static void FallbackAssetReturnPreferenceViewShowsTheStatusInCapitals()
    {
        var item = FallbackAssetReturnPreference.NewFallbackAssetReturnPreference("code-421", "label-508");
        var view = FallbackAssetReturnPreferenceView.ToFallbackAssetReturnPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAssetReturnPreferenceView.ToFallbackAssetReturnPreferenceViews(new List<FallbackAssetReturnPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAssetReturnPreferenceViewShowsTheStatusInCapitals();
    }
}
