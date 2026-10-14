namespace Acme.Workforce.Workplace.Tests.FloorPlan.Assignment;

using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FloorPlanWindowStoreChecks
{
    public static void MemoryFloorPlanWindowStoreFindsWhatItSaved()
    {
        var store = FloorPlanWindowStore.NewMemoryFloorPlanWindowStore();
        var item = FloorPlanWindow.NewFloorPlanWindow("code-592", "name-638");
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
        MemoryFloorPlanWindowStoreFindsWhatItSaved();
    }
}
