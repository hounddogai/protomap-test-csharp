namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolSegmentStoreChecks
{
    public static void MemoryPoolSegmentStoreFindsWhatItSaved()
    {
        var store = PoolSegmentStore.NewMemoryPoolSegmentStore();
        var item = PoolSegment.NewPoolSegment("code-338", "title-125");
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
        MemoryPoolSegmentStoreFindsWhatItSaved();
    }
}
