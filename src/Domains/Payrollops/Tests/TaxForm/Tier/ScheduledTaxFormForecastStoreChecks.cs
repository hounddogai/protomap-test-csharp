namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledTaxFormForecastStoreChecks
{
    public static void MemoryScheduledTaxFormForecastStoreFindsWhatItSaved()
    {
        var store = ScheduledTaxFormForecastStore.NewMemoryScheduledTaxFormForecastStore();
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
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
        MemoryScheduledTaxFormForecastStoreFindsWhatItSaved();
    }
}
