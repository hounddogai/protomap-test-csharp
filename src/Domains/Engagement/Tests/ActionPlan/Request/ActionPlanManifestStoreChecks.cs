namespace Acme.Workforce.Engagement.Tests.ActionPlan.Request;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ActionPlanManifestStoreChecks
{
    public static void MemoryActionPlanManifestStoreFindsWhatItSaved()
    {
        var store = ActionPlanManifestStore.NewMemoryActionPlanManifestStore();
        var item = ActionPlanManifest.NewActionPlanManifest("code-535", "title-802");
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
        MemoryActionPlanManifestStoreFindsWhatItSaved();
    }
}
