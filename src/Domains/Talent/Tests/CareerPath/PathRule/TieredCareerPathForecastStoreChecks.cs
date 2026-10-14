namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class TieredCareerPathForecastStoreChecks
{
    public static void MemoryTieredCareerPathForecastStoreFindsWhatItSaved()
    {
        var store = TieredCareerPathForecastStore.NewMemoryTieredCareerPathForecastStore();
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("code-777", "name-110");
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
        MemoryTieredCareerPathForecastStoreFindsWhatItSaved();
    }
}
