namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanManifestStoreChecks
{
    public static void MemorySuccessionPlanManifestStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanManifestStore.NewMemorySuccessionPlanManifestStore();
        var item = SuccessionPlanManifest.NewSuccessionPlanManifest("code-899", "label-794");
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
        MemorySuccessionPlanManifestStoreFindsWhatItSaved();
    }
}
