namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastStoreChecks
{
    public static void MemoryStrictRateForecastStoreFindsWhatItSaved()
    {
        var store = StrictRateForecastStore.NewMemoryStrictRateForecastStore();
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
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
        MemoryStrictRateForecastStoreFindsWhatItSaved();
    }
}
