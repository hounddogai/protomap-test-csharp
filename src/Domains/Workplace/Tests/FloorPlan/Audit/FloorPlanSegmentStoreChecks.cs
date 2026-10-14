namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanSegmentStoreChecks
{
    public static void MemoryFloorPlanSegmentStoreFindsWhatItSaved()
    {
        var store = FloorPlanSegmentStore.NewMemoryFloorPlanSegmentStore();
        var item = FloorPlanSegment.NewFloorPlanSegment("code-629", "title-649");
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
        MemoryFloorPlanSegmentStoreFindsWhatItSaved();
    }
}
