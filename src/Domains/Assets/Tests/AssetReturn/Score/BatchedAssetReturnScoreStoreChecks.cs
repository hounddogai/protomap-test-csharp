namespace Acme.Workforce.Assets.Tests.AssetReturn.Score;

using Acme.Workforce.Assets.AssetReturn.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedAssetReturnScoreStoreChecks
{
    public static void MemoryBatchedAssetReturnScoreStoreFindsWhatItSaved()
    {
        var store = BatchedAssetReturnScoreStore.NewMemoryBatchedAssetReturnScoreStore();
        var item = BatchedAssetReturnScore.NewBatchedAssetReturnScore("code-402", "name-416");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedAssetReturnScoreStoreFindsWhatItSaved();
    }
}
