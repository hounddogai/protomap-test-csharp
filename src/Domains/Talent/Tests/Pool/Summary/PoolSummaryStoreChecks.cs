namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolSummaryStoreChecks
{
    public static void MemoryPoolSummaryStoreFindsWhatItSaved()
    {
        var store = PoolSummaryStore.NewMemoryPoolSummaryStore();
        var item = PoolSummary.NewPoolSummary("code-959", "name-632");
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
        MemoryPoolSummaryStoreFindsWhatItSaved();
    }
}
