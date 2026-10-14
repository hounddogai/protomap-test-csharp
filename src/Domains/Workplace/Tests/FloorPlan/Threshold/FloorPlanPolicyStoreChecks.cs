namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanPolicyStoreChecks
{
    public static void MemoryFloorPlanPolicyStoreFindsWhatItSaved()
    {
        var store = FloorPlanPolicyStore.NewMemoryFloorPlanPolicyStore();
        var item = FloorPlanPolicy.NewFloorPlanPolicy("code-596", "title-501");
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
        MemoryFloorPlanPolicyStoreFindsWhatItSaved();
    }
}
