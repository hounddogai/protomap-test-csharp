namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictOffboardingForecastStoreChecks
{
    public static void MemoryStrictOffboardingForecastStoreFindsWhatItSaved()
    {
        var store = StrictOffboardingForecastStore.NewMemoryStrictOffboardingForecastStore();
        var item = StrictOffboardingForecast.NewStrictOffboardingForecast("code-838", "title-913");
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
        MemoryStrictOffboardingForecastStoreFindsWhatItSaved();
    }
}
