namespace Acme.Workforce.Contractors.Tests.Offboarding.Forecast;

using Acme.Workforce.Contractors.Offboarding.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class OffboardingForecastStoreChecks
{
    public static void MemoryOffboardingForecastStoreFindsWhatItSaved()
    {
        var store = OffboardingForecastStore.NewMemoryOffboardingForecastStore();
        var item = OffboardingForecast.NewOffboardingForecast("code-119", "label-128");
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
        MemoryOffboardingForecastStoreFindsWhatItSaved();
    }
}
