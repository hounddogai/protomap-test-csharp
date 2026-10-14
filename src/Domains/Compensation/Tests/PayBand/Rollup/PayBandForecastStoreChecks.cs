namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastStoreChecks
{
    public static void MemoryPayBandForecastStoreFindsWhatItSaved()
    {
        var store = PayBandForecastStore.NewMemoryPayBandForecastStore();
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
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
        MemoryPayBandForecastStoreFindsWhatItSaved();
    }
}
