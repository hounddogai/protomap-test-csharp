namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class TieredExceptionGroupStoreChecks
{
    public static void MemoryTieredExceptionGroupStoreFindsWhatItSaved()
    {
        var store = TieredExceptionGroupStore.NewMemoryTieredExceptionGroupStore();
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
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
        MemoryTieredExceptionGroupStoreFindsWhatItSaved();
    }
}
