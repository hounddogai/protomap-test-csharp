namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowStoreChecks
{
    public static void MemoryPoolWindowStoreFindsWhatItSaved()
    {
        var store = PoolWindowStore.NewMemoryPoolWindowStore();
        var item = PoolWindow.NewPoolWindow("code-431", "title-338");
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
        MemoryPoolWindowStoreFindsWhatItSaved();
    }
}
