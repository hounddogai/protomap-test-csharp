namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WeightedReturnPlanManifestStoreChecks
{
    public static void MemoryWeightedReturnPlanManifestStoreFindsWhatItSaved()
    {
        var store = WeightedReturnPlanManifestStore.NewMemoryWeightedReturnPlanManifestStore();
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
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
        MemoryWeightedReturnPlanManifestStoreFindsWhatItSaved();
    }
}
