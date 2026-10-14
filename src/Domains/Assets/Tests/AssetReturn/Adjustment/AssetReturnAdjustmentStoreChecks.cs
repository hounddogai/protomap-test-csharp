namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnAdjustmentStoreChecks
{
    public static void MemoryAssetReturnAdjustmentStoreFindsWhatItSaved()
    {
        var store = AssetReturnAdjustmentStore.NewMemoryAssetReturnAdjustmentStore();
        var item = AssetReturnAdjustment.NewAssetReturnAdjustment("code-775", "title-240");
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
        MemoryAssetReturnAdjustmentStoreFindsWhatItSaved();
    }
}
