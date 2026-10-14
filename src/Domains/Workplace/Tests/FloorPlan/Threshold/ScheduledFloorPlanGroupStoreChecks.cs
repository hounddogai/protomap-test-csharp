namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledFloorPlanGroupStoreChecks
{
    public static void MemoryScheduledFloorPlanGroupStoreFindsWhatItSaved()
    {
        var store = ScheduledFloorPlanGroupStore.NewMemoryScheduledFloorPlanGroupStore();
        var item = ScheduledFloorPlanGroup.NewScheduledFloorPlanGroup("code-593", "title-230");
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
        MemoryScheduledFloorPlanGroupStoreFindsWhatItSaved();
    }
}
