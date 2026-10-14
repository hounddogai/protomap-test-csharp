namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorBatchStoreChecks
{
    public static void MemoryVisitorBatchStoreFindsWhatItSaved()
    {
        var store = VisitorBatchStore.NewMemoryVisitorBatchStore();
        var item = VisitorBatch.NewVisitorBatch("code-361", "title-289");
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
        MemoryVisitorBatchStoreFindsWhatItSaved();
    }
}
