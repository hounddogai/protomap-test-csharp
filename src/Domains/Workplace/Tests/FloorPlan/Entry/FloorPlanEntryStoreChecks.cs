namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanEntryStoreChecks
{
    public static void MemoryFloorPlanEntryStoreFindsWhatItSaved()
    {
        var store = FloorPlanEntryStore.NewMemoryFloorPlanEntryStore();
        var item = FloorPlanEntry.NewFloorPlanEntry("code-465", "name-482");
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
        MemoryFloorPlanEntryStoreFindsWhatItSaved();
    }
}
