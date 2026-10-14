namespace Acme.Workforce.Assets.Tests.AssetReturn.Estimate;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class WeightedAssetReturnForecastStoreChecks
{
    public static void MemoryWeightedAssetReturnForecastStoreFindsWhatItSaved()
    {
        var store = WeightedAssetReturnForecastStore.NewMemoryWeightedAssetReturnForecastStore();
        var item = WeightedAssetReturnForecast.NewWeightedAssetReturnForecast("code-30", "name-731");
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
        MemoryWeightedAssetReturnForecastStoreFindsWhatItSaved();
    }
}
