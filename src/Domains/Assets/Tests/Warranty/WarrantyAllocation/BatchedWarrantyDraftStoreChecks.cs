namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class BatchedWarrantyDraftStoreChecks
{
    public static void MemoryBatchedWarrantyDraftStoreFindsWhatItSaved()
    {
        var store = BatchedWarrantyDraftStore.NewMemoryBatchedWarrantyDraftStore();
        var item = BatchedWarrantyDraft.NewBatchedWarrantyDraft("code-626", "title-996");
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
        MemoryBatchedWarrantyDraftStoreFindsWhatItSaved();
    }
}
