namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PoolBundleStoreChecks
{
    public static void MemoryPoolBundleStoreFindsWhatItSaved()
    {
        var store = PoolBundleStore.NewMemoryPoolBundleStore();
        var item = PoolBundle.NewPoolBundle("code-523", "name-854");
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
        MemoryPoolBundleStoreFindsWhatItSaved();
    }
}
