namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionForecastStoreChecks
{
    public static void MemoryPositionForecastStoreFindsWhatItSaved()
    {
        var store = PositionForecastStore.NewMemoryPositionForecastStore();
        var item = PositionForecast.NewPositionForecast("code-415", "label-75");
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
        MemoryPositionForecastStoreFindsWhatItSaved();
    }
}
