namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class LegacyOffboardingForecastStoreChecks
{
    public static void MemoryLegacyOffboardingForecastStoreFindsWhatItSaved()
    {
        var store = LegacyOffboardingForecastStore.NewMemoryLegacyOffboardingForecastStore();
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
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
        MemoryLegacyOffboardingForecastStoreFindsWhatItSaved();
    }
}
