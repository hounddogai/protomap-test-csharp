namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleForecastStoreChecks
{
    public static void MemoryMeritCycleForecastStoreFindsWhatItSaved()
    {
        var store = MeritCycleForecastStore.NewMemoryMeritCycleForecastStore();
        var item = MeritCycleForecast.NewMeritCycleForecast("code-807", "title-23");
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
        MemoryMeritCycleForecastStoreFindsWhatItSaved();
    }
}
