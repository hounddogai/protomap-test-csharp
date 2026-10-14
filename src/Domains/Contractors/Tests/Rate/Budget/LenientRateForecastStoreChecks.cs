namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LenientRateForecastStoreChecks
{
    public static void MemoryLenientRateForecastStoreFindsWhatItSaved()
    {
        var store = LenientRateForecastStore.NewMemoryLenientRateForecastStore();
        var item = LenientRateForecast.NewLenientRateForecast("code-751", "label-237");
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
        MemoryLenientRateForecastStoreFindsWhatItSaved();
    }
}
