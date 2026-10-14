namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayForecastStoreChecks
{
    public static void MemoryRetroPayForecastStoreFindsWhatItSaved()
    {
        var store = RetroPayForecastStore.NewMemoryRetroPayForecastStore();
        var item = RetroPayForecast.NewRetroPayForecast("code-777", "title-1");
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
        MemoryRetroPayForecastStoreFindsWhatItSaved();
    }
}
