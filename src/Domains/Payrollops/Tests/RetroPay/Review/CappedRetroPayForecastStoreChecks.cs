namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedRetroPayForecastStoreChecks
{
    public static void MemoryCappedRetroPayForecastStoreFindsWhatItSaved()
    {
        var store = CappedRetroPayForecastStore.NewMemoryCappedRetroPayForecastStore();
        var item = CappedRetroPayForecast.NewCappedRetroPayForecast("code-620", "name-378");
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
        MemoryCappedRetroPayForecastStoreFindsWhatItSaved();
    }
}
