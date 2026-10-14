namespace Acme.Workforce.Assets.Tests.AssetReturn.Budget;

using Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnDraftViewChecks
{
    public static void AdaptiveAssetReturnDraftViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAssetReturnDraft.NewAdaptiveAssetReturnDraft("code-741", "label-307");
        var view = AdaptiveAssetReturnDraftView.ToAdaptiveAssetReturnDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAssetReturnDraftView.ToAdaptiveAssetReturnDraftViews(new List<AdaptiveAssetReturnDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnDraftViewShowsTheStatusInCapitals();
    }
}
