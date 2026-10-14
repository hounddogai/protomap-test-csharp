namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SuccessionPlanForecastStoreChecks
{
    public static void MemorySuccessionPlanForecastStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanForecastStore.NewMemorySuccessionPlanForecastStore();
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("code-853", "label-788");
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
        MemorySuccessionPlanForecastStoreFindsWhatItSaved();
    }
}
