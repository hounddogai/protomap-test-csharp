namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RegionalBenchmarkForecastStoreChecks
{
    public static void MemoryRegionalBenchmarkForecastStoreFindsWhatItSaved()
    {
        var store = RegionalBenchmarkForecastStore.NewMemoryRegionalBenchmarkForecastStore();
        var item = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast("code-464", "name-7");
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
        MemoryRegionalBenchmarkForecastStoreFindsWhatItSaved();
    }
}
