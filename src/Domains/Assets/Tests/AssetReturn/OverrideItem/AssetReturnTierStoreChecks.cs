namespace Acme.Workforce.Assets.Tests.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AssetReturnTierStoreChecks
{
    public static void MemoryAssetReturnTierStoreFindsWhatItSaved()
    {
        var store = AssetReturnTierStore.NewMemoryAssetReturnTierStore();
        var item = AssetReturnTier.NewAssetReturnTier("code-151", "label-997");
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
        MemoryAssetReturnTierStoreFindsWhatItSaved();
    }
}
