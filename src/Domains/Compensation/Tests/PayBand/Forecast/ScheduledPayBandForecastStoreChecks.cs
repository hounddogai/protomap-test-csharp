namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastStoreChecks
{
    public static void MemoryScheduledPayBandForecastStoreFindsWhatItSaved()
    {
        var store = ScheduledPayBandForecastStore.NewMemoryScheduledPayBandForecastStore();
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
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
        MemoryScheduledPayBandForecastStoreFindsWhatItSaved();
    }
}
