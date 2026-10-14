namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentForecastStoreChecks
{
    public static void MemoryGarnishmentForecastStoreFindsWhatItSaved()
    {
        var store = GarnishmentForecastStore.NewMemoryGarnishmentForecastStore();
        var item = GarnishmentForecast.NewGarnishmentForecast("code-406", "name-44");
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
        MemoryGarnishmentForecastStoreFindsWhatItSaved();
    }
}
