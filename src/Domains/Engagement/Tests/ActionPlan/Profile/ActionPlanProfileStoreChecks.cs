namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ActionPlanProfileStoreChecks
{
    public static void MemoryActionPlanProfileStoreFindsWhatItSaved()
    {
        var store = ActionPlanProfileStore.NewMemoryActionPlanProfileStore();
        var item = ActionPlanProfile.NewActionPlanProfile("code-844", "title-493");
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
        MemoryActionPlanProfileStoreFindsWhatItSaved();
    }
}
