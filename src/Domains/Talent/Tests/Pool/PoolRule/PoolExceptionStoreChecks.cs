namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class PoolExceptionStoreChecks
{
    public static void MemoryPoolExceptionStoreFindsWhatItSaved()
    {
        var store = PoolExceptionStore.NewMemoryPoolExceptionStore();
        var item = PoolException.NewPoolException("code-737", "title-296");
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
        MemoryPoolExceptionStoreFindsWhatItSaved();
    }
}
