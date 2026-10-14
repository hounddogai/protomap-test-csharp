namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PoolProfileStoreChecks
{
    public static void MemoryPoolProfileStoreFindsWhatItSaved()
    {
        var store = PoolProfileStore.NewMemoryPoolProfileStore();
        var item = PoolProfile.NewPoolProfile("code-14", "label-680");
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
        MemoryPoolProfileStoreFindsWhatItSaved();
    }
}
