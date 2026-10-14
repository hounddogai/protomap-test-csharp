namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultFloorPlanSegmentStoreChecks
{
    public static void MemoryDefaultFloorPlanSegmentStoreFindsWhatItSaved()
    {
        var store = DefaultFloorPlanSegmentStore.NewMemoryDefaultFloorPlanSegmentStore();
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
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
        MemoryDefaultFloorPlanSegmentStoreFindsWhatItSaved();
    }
}
