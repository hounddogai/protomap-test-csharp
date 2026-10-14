namespace Acme.Workforce.Assets.Tests.AssetReturn.Rule;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnVarianceStoreChecks
{
    public static void MemoryAdaptiveAssetReturnVarianceStoreFindsWhatItSaved()
    {
        var store = AdaptiveAssetReturnVarianceStore.NewMemoryAdaptiveAssetReturnVarianceStore();
        var item = AdaptiveAssetReturnVariance.NewAdaptiveAssetReturnVariance("code-870", "name-802");
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
        MemoryAdaptiveAssetReturnVarianceStoreFindsWhatItSaved();
    }
}
