namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalWarrantyLedgerStoreChecks
{
    public static void MemoryRegionalWarrantyLedgerStoreFindsWhatItSaved()
    {
        var store = RegionalWarrantyLedgerStore.NewMemoryRegionalWarrantyLedgerStore();
        var item = RegionalWarrantyLedger.NewRegionalWarrantyLedger("code-688", "title-360");
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
        MemoryRegionalWarrantyLedgerStoreFindsWhatItSaved();
    }
}
