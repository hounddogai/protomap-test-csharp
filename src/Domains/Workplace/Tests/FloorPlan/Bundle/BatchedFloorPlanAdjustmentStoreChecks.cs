namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedFloorPlanAdjustmentStoreChecks
{
    public static void MemoryBatchedFloorPlanAdjustmentStoreFindsWhatItSaved()
    {
        var store = BatchedFloorPlanAdjustmentStore.NewMemoryBatchedFloorPlanAdjustmentStore();
        var item = BatchedFloorPlanAdjustment.NewBatchedFloorPlanAdjustment("code-913", "name-230");
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
        MemoryBatchedFloorPlanAdjustmentStoreFindsWhatItSaved();
    }
}
