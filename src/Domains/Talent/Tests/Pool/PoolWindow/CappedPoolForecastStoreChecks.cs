namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class CappedPoolForecastStoreChecks
{
    public static void MemoryCappedPoolForecastStoreFindsWhatItSaved()
    {
        var store = CappedPoolForecastStore.NewMemoryCappedPoolForecastStore();
        var item = CappedPoolForecast.NewCappedPoolForecast("code-543", "name-991");
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
        MemoryCappedPoolForecastStoreFindsWhatItSaved();
    }
}
