namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PrimaryDeskForecastStoreChecks
{
    public static void MemoryPrimaryDeskForecastStoreFindsWhatItSaved()
    {
        var store = PrimaryDeskForecastStore.NewMemoryPrimaryDeskForecastStore();
        var item = PrimaryDeskForecast.NewPrimaryDeskForecast("code-737", "label-320");
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
        MemoryPrimaryDeskForecastStoreFindsWhatItSaved();
    }
}
