namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedAssetReturnCycleStoreChecks
{
    public static void MemoryCappedAssetReturnCycleStoreFindsWhatItSaved()
    {
        var store = CappedAssetReturnCycleStore.NewMemoryCappedAssetReturnCycleStore();
        var item = CappedAssetReturnCycle.NewCappedAssetReturnCycle("code-567", "name-357");
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
        MemoryCappedAssetReturnCycleStoreFindsWhatItSaved();
    }
}
