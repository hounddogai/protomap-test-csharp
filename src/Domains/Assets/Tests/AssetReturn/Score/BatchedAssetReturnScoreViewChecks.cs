namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedAssetReturnScoreViewChecks
{
    public static void BatchedAssetReturnScoreViewShowsTheStatusInCapitals()
    {
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        var view = BatchedAssetReturnScoreView.ToBatchedAssetReturnScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAssetReturnScoreView.ToBatchedAssetReturnScoreViews(new List<BatchedAssetReturnScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAssetReturnScoreViewShowsTheStatusInCapitals();
    }
}
