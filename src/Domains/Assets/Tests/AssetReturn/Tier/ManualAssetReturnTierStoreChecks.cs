namespace Acme.Workforce.Assets.Tests.AssetReturn.Tier;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualAssetReturnTierStoreChecks
{
    public static void MemoryManualAssetReturnTierStoreFindsWhatItSaved()
    {
        var store = ManualAssetReturnTierStore.NewMemoryManualAssetReturnTierStore();
        var item = ManualAssetReturnTier.NewManualAssetReturnTier("code-500", "title-806");
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
        MemoryManualAssetReturnTierStoreFindsWhatItSaved();
    }
}
