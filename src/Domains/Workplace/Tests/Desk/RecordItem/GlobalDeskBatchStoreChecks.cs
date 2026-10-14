namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalDeskBatchStoreChecks
{
    public static void MemoryGlobalDeskBatchStoreFindsWhatItSaved()
    {
        var store = GlobalDeskBatchStore.NewMemoryGlobalDeskBatchStore();
        var item = GlobalDeskBatch.NewGlobalDeskBatch("code-606", "label-174");
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
        MemoryGlobalDeskBatchStoreFindsWhatItSaved();
    }
}
