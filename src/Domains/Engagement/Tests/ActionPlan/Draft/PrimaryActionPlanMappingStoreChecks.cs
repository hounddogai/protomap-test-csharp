namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PrimaryActionPlanMappingStoreChecks
{
    public static void MemoryPrimaryActionPlanMappingStoreFindsWhatItSaved()
    {
        var store = PrimaryActionPlanMappingStore.NewMemoryPrimaryActionPlanMappingStore();
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
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
        MemoryPrimaryActionPlanMappingStoreFindsWhatItSaved();
    }
}
