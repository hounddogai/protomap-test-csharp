namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastStoreChecks
{
    public static void MemoryLenientOffboardingForecastStoreFindsWhatItSaved()
    {
        var store = LenientOffboardingForecastStore.NewMemoryLenientOffboardingForecastStore();
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
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
        MemoryLenientOffboardingForecastStoreFindsWhatItSaved();
    }
}
