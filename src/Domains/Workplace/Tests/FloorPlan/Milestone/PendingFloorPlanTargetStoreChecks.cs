namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PendingFloorPlanTargetStoreChecks
{
    public static void MemoryPendingFloorPlanTargetStoreFindsWhatItSaved()
    {
        var store = PendingFloorPlanTargetStore.NewMemoryPendingFloorPlanTargetStore();
        var item = PendingFloorPlanTarget.NewPendingFloorPlanTarget("code-427", "label-236");
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
        MemoryPendingFloorPlanTargetStoreFindsWhatItSaved();
    }
}
