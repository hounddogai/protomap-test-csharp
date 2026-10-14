namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedAssetReturnScoreChecks
{
    public static void BatchedAssetReturnScoreStartsInCreated()
    {
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        Check.Equal(item.Status, BatchedAssetReturnScoreStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAssetReturnScoreMovesToPublished()
    {
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        try
        {
            item.MoveTo(BatchedAssetReturnScoreStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAssetReturnScoreStatus.Published, "status");
    }

    public static void BatchedAssetReturnScoreRejectsMovesFromRetired()
    {
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        item.Status = BatchedAssetReturnScoreStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedAssetReturnScoreStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedAssetReturnScoreStartsInCreated();
        BatchedAssetReturnScoreMovesToPublished();
        BatchedAssetReturnScoreRejectsMovesFromRetired();
    }
}
