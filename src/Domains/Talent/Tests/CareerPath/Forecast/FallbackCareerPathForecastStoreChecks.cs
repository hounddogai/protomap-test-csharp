namespace Acme.Workforce.Talent.Tests.CareerPath.Forecast;

using Acme.Workforce.Talent.CareerPath.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class FallbackCareerPathForecastStoreChecks
{
    public static void MemoryFallbackCareerPathForecastStoreFindsWhatItSaved()
    {
        var store = FallbackCareerPathForecastStore.NewMemoryFallbackCareerPathForecastStore();
        var item = FallbackCareerPathForecast.NewFallbackCareerPathForecast("code-976", "label-766");
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
        MemoryFallbackCareerPathForecastStoreFindsWhatItSaved();
    }
}
