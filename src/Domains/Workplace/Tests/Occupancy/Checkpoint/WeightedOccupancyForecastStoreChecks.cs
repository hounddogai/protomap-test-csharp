namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WeightedOccupancyForecastStoreChecks
{
    public static void MemoryWeightedOccupancyForecastStoreFindsWhatItSaved()
    {
        var store = WeightedOccupancyForecastStore.NewMemoryWeightedOccupancyForecastStore();
        var item = WeightedOccupancyForecast.NewWeightedOccupancyForecast("code-661", "name-625");
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
        MemoryWeightedOccupancyForecastStoreFindsWhatItSaved();
    }
}
