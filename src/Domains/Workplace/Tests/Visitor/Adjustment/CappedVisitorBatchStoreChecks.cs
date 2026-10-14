namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorBatchStoreChecks
{
    public static void MemoryCappedVisitorBatchStoreFindsWhatItSaved()
    {
        var store = CappedVisitorBatchStore.NewMemoryCappedVisitorBatchStore();
        var item = CappedVisitorBatch.NewCappedVisitorBatch("code-677", "title-142");
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
        MemoryCappedVisitorBatchStoreFindsWhatItSaved();
    }
}
