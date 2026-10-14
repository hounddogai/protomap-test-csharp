namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanAllocationStoreChecks
{
    public static void MemoryActionPlanAllocationStoreFindsWhatItSaved()
    {
        var store = ActionPlanAllocationStore.NewMemoryActionPlanAllocationStore();
        var item = ActionPlanAllocation.NewActionPlanAllocation("code-546", "label-956");
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
        MemoryActionPlanAllocationStoreFindsWhatItSaved();
    }
}
