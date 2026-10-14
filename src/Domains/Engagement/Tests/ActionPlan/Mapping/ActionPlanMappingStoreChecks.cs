namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanMappingStoreChecks
{
    public static void MemoryActionPlanMappingStoreFindsWhatItSaved()
    {
        var store = ActionPlanMappingStore.NewMemoryActionPlanMappingStore();
        var item = ActionPlanMapping.NewActionPlanMapping("code-881", "title-627");
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
        MemoryActionPlanMappingStoreFindsWhatItSaved();
    }
}
