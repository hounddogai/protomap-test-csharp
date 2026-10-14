namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanManifestStoreChecks
{
    public static void MemoryFallbackSuccessionPlanManifestStoreFindsWhatItSaved()
    {
        var store = FallbackSuccessionPlanManifestStore.NewMemoryFallbackSuccessionPlanManifestStore();
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("code-353", "name-283");
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
        MemoryFallbackSuccessionPlanManifestStoreFindsWhatItSaved();
    }
}
