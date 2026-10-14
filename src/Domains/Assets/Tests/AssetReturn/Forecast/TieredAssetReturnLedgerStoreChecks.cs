namespace Acme.Workforce.Assets.Tests.AssetReturn.Forecast;

using Acme.Workforce.Assets.AssetReturn.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredAssetReturnLedgerStoreChecks
{
    public static void MemoryTieredAssetReturnLedgerStoreFindsWhatItSaved()
    {
        var store = TieredAssetReturnLedgerStore.NewMemoryTieredAssetReturnLedgerStore();
        var item = TieredAssetReturnLedger.NewTieredAssetReturnLedger("code-206", "title-906");
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
        MemoryTieredAssetReturnLedgerStoreFindsWhatItSaved();
    }
}
