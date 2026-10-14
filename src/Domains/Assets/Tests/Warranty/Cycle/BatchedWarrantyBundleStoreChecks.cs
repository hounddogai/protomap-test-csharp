namespace Acme.Workforce.Assets.Tests.Warranty.Cycle;

using Acme.Workforce.Assets.Warranty.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedWarrantyBundleStoreChecks
{
    public static void MemoryBatchedWarrantyBundleStoreFindsWhatItSaved()
    {
        var store = BatchedWarrantyBundleStore.NewMemoryBatchedWarrantyBundleStore();
        var item = BatchedWarrantyBundle.NewBatchedWarrantyBundle("code-774", "name-656");
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
        MemoryBatchedWarrantyBundleStoreFindsWhatItSaved();
    }
}
