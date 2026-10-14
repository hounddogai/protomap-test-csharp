namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssetReturnEstimateStoreChecks
{
    public static void MemoryAssetReturnEstimateStoreFindsWhatItSaved()
    {
        var store = AssetReturnEstimateStore.NewMemoryAssetReturnEstimateStore();
        var item = AssetReturnEstimate.NewAssetReturnEstimate("code-296", "title-603");
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
        MemoryAssetReturnEstimateStoreFindsWhatItSaved();
    }
}
