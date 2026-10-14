namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AssetReturnLedgerStoreChecks
{
    public static void MemoryAssetReturnLedgerStoreFindsWhatItSaved()
    {
        var store = AssetReturnLedgerStore.NewMemoryAssetReturnLedgerStore();
        var item = AssetReturnLedger.NewAssetReturnLedger("code-31", "title-673");
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
        MemoryAssetReturnLedgerStoreFindsWhatItSaved();
    }
}
