namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolOverrideStoreChecks
{
    public static void MemoryPoolOverrideStoreFindsWhatItSaved()
    {
        var store = PoolOverrideStore.NewMemoryPoolOverrideStore();
        var item = PoolOverride.NewPoolOverride("code-101", "name-262");
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
        MemoryPoolOverrideStoreFindsWhatItSaved();
    }
}
