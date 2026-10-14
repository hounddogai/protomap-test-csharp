namespace Acme.Workforce.Assets.Tests.AssetReturn.Request;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackAssetReturnSegmentViewChecks
{
    public static void FallbackAssetReturnSegmentViewShowsTheStatusInCapitals()
    {
        var item = FallbackAssetReturnSegment.NewFallbackAssetReturnSegment("code-403", "title-868");
        var view = FallbackAssetReturnSegmentView.ToFallbackAssetReturnSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAssetReturnSegmentView.ToFallbackAssetReturnSegmentViews(new List<FallbackAssetReturnSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAssetReturnSegmentViewShowsTheStatusInCapitals();
    }
}
