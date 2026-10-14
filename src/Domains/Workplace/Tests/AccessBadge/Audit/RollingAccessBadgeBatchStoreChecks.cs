namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingAccessBadgeBatchStoreChecks
{
    public static void MemoryRollingAccessBadgeBatchStoreFindsWhatItSaved()
    {
        var store = RollingAccessBadgeBatchStore.NewMemoryRollingAccessBadgeBatchStore();
        var item = RollingAccessBadgeBatch.NewRollingAccessBadgeBatch("code-701", "title-332");
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
        MemoryRollingAccessBadgeBatchStoreFindsWhatItSaved();
    }
}
