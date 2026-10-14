namespace Acme.Workforce.Assets.Tests.AssetReturn.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AssetReturnQuotaStoreChecks
{
    public static void MemoryAssetReturnQuotaStoreFindsWhatItSaved()
    {
        var store = AssetReturnQuotaStore.NewMemoryAssetReturnQuotaStore();
        var item = AssetReturnQuota.NewAssetReturnQuota("code-10", "name-668");
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
        MemoryAssetReturnQuotaStoreFindsWhatItSaved();
    }
}
