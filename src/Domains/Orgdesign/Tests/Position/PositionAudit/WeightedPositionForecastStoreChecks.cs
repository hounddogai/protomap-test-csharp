namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class WeightedPositionForecastStoreChecks
{
    public static void MemoryWeightedPositionForecastStoreFindsWhatItSaved()
    {
        var store = WeightedPositionForecastStore.NewMemoryWeightedPositionForecastStore();
        var item = WeightedPositionForecast.NewWeightedPositionForecast("code-219", "label-757");
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
        MemoryWeightedPositionForecastStoreFindsWhatItSaved();
    }
}
