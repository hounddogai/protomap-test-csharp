namespace Acme.Workforce.Assets.Tests.AssetReturn.Segment;

using Acme.Workforce.Assets.AssetReturn.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictAssetReturnSegmentViewChecks
{
    public static void StrictAssetReturnSegmentViewShowsTheStatusInCapitals()
    {
        var item = StrictAssetReturnSegment.NewStrictAssetReturnSegment("code-122", "label-577");
        var view = StrictAssetReturnSegmentView.ToStrictAssetReturnSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAssetReturnSegmentView.ToStrictAssetReturnSegmentViews(new List<StrictAssetReturnSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAssetReturnSegmentViewShowsTheStatusInCapitals();
    }
}
