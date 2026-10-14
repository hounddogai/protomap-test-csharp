namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TieredPoolGroupStoreChecks
{
    public static void MemoryTieredPoolGroupStoreFindsWhatItSaved()
    {
        var store = TieredPoolGroupStore.NewMemoryTieredPoolGroupStore();
        var item = TieredPoolGroup.NewTieredPoolGroup("code-987", "name-790");
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
        MemoryTieredPoolGroupStoreFindsWhatItSaved();
    }
}
