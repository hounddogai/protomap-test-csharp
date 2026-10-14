namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolForecastStoreChecks
{
    public static void MemoryPoolForecastStoreFindsWhatItSaved()
    {
        var store = PoolForecastStore.NewMemoryPoolForecastStore();
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
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
        MemoryPoolForecastStoreFindsWhatItSaved();
    }
}
