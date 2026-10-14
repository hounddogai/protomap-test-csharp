namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryStoreChecks
{
    public static void MemoryScheduledFloorPlanEntryStoreFindsWhatItSaved()
    {
        var store = ScheduledFloorPlanEntryStore.NewMemoryScheduledFloorPlanEntryStore();
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
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
        MemoryScheduledFloorPlanEntryStoreFindsWhatItSaved();
    }
}
