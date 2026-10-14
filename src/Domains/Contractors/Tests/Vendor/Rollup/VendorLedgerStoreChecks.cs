namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorLedgerStoreChecks
{
    public static void MemoryVendorLedgerStoreFindsWhatItSaved()
    {
        var store = VendorLedgerStore.NewMemoryVendorLedgerStore();
        var item = VendorLedger.NewVendorLedger("code-639", "title-65");
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
        MemoryVendorLedgerStoreFindsWhatItSaved();
    }
}
