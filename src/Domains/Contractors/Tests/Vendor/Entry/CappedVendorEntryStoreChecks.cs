namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedVendorEntryStoreChecks
{
    public static void MemoryCappedVendorEntryStoreFindsWhatItSaved()
    {
        var store = CappedVendorEntryStore.NewMemoryCappedVendorEntryStore();
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
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
        MemoryCappedVendorEntryStoreFindsWhatItSaved();
    }
}
