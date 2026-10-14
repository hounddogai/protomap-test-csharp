namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientAssetReturnTargetViewChecks
{
    public static void LenientAssetReturnTargetViewShowsTheStatusInCapitals()
    {
        var item = LenientAssetReturnTarget.NewLenientAssetReturnTarget("code-566", "name-521");
        var view = LenientAssetReturnTargetView.ToLenientAssetReturnTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientAssetReturnTargetView.ToLenientAssetReturnTargetViews(new List<LenientAssetReturnTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientAssetReturnTargetViewShowsTheStatusInCapitals();
    }
}
