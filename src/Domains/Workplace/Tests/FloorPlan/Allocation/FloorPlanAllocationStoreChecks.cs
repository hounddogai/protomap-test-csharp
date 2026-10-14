namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanAllocationStoreChecks
{
    public static void MemoryFloorPlanAllocationStoreFindsWhatItSaved()
    {
        var store = FloorPlanAllocationStore.NewMemoryFloorPlanAllocationStore();
        var item = FloorPlanAllocation.NewFloorPlanAllocation("code-584", "label-231");
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
        MemoryFloorPlanAllocationStoreFindsWhatItSaved();
    }
}
