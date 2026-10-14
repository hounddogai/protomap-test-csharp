namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualForecastStoreChecks
{
    public static void MemoryAccrualForecastStoreFindsWhatItSaved()
    {
        var store = AccrualForecastStore.NewMemoryAccrualForecastStore();
        var item = AccrualForecast.NewAccrualForecast("code-617", "name-440");
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
        MemoryAccrualForecastStoreFindsWhatItSaved();
    }
}
