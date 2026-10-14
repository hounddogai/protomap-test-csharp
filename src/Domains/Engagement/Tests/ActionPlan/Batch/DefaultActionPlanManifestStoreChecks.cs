namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultActionPlanManifestStoreChecks
{
    public static void MemoryDefaultActionPlanManifestStoreFindsWhatItSaved()
    {
        var store = DefaultActionPlanManifestStore.NewMemoryDefaultActionPlanManifestStore();
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("code-988", "name-485");
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
        MemoryDefaultActionPlanManifestStoreFindsWhatItSaved();
    }
}
