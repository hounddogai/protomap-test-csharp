namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountBatchStoreChecks
{
    public static void MemoryHeadcountBatchStoreFindsWhatItSaved()
    {
        var store = HeadcountBatchStore.NewMemoryHeadcountBatchStore();
        var item = HeadcountBatch.NewHeadcountBatch("code-460", "name-204");
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
        MemoryHeadcountBatchStoreFindsWhatItSaved();
    }
}
