namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class StrictAccrualForecastStoreChecks
{
    public static void MemoryStrictAccrualForecastStoreFindsWhatItSaved()
    {
        var store = StrictAccrualForecastStore.NewMemoryStrictAccrualForecastStore();
        var item = StrictAccrualForecast.NewStrictAccrualForecast("code-214", "name-613");
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
        MemoryStrictAccrualForecastStoreFindsWhatItSaved();
    }
}
