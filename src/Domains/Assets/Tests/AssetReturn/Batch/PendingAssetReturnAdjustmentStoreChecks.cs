namespace Acme.Workforce.Assets.Tests.AssetReturn.Batch;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAssetReturnAdjustmentStoreChecks
{
    public static void MemoryPendingAssetReturnAdjustmentStoreFindsWhatItSaved()
    {
        var store = PendingAssetReturnAdjustmentStore.NewMemoryPendingAssetReturnAdjustmentStore();
        var item = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment("code-331", "title-474");
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
        MemoryPendingAssetReturnAdjustmentStoreFindsWhatItSaved();
    }
}
