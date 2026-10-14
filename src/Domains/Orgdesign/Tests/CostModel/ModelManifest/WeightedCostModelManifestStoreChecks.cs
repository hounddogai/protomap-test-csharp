namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class WeightedCostModelManifestStoreChecks
{
    public static void MemoryWeightedCostModelManifestStoreFindsWhatItSaved()
    {
        var store = WeightedCostModelManifestStore.NewMemoryWeightedCostModelManifestStore();
        var item = WeightedCostModelManifest.NewWeightedCostModelManifest("code-284", "name-702");
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
        MemoryWeightedCostModelManifestStoreFindsWhatItSaved();
    }
}
