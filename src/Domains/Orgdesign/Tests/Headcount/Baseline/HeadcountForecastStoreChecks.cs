namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountForecastStoreChecks
{
    public static void MemoryHeadcountForecastStoreFindsWhatItSaved()
    {
        var store = HeadcountForecastStore.NewMemoryHeadcountForecastStore();
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
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
        MemoryHeadcountForecastStoreFindsWhatItSaved();
    }
}
