namespace Acme.Workforce.Assets.Tests.Warranty.RecordItem;

using Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedWarrantyRecordStoreChecks
{
    public static void MemoryBatchedWarrantyRecordStoreFindsWhatItSaved()
    {
        var store = BatchedWarrantyRecordStore.NewMemoryBatchedWarrantyRecordStore();
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("code-85", "title-518");
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
        MemoryBatchedWarrantyRecordStoreFindsWhatItSaved();
    }
}
