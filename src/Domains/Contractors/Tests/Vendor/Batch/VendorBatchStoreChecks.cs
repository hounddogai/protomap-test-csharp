namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorBatchStoreChecks
{
    public static void MemoryVendorBatchStoreFindsWhatItSaved()
    {
        var store = VendorBatchStore.NewMemoryVendorBatchStore();
        var item = VendorBatch.NewVendorBatch("code-257", "label-849");
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
        MemoryVendorBatchStoreFindsWhatItSaved();
    }
}
