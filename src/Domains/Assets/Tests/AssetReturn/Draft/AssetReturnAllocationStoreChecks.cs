namespace Acme.Workforce.Assets.Tests.AssetReturn.Draft;

using Acme.Workforce.Assets.AssetReturn.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AssetReturnAllocationStoreChecks
{
    public static void MemoryAssetReturnAllocationStoreFindsWhatItSaved()
    {
        var store = AssetReturnAllocationStore.NewMemoryAssetReturnAllocationStore();
        var item = AssetReturnAllocation.NewAssetReturnAllocation("code-331", "name-362");
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
        MemoryAssetReturnAllocationStoreFindsWhatItSaved();
    }
}
