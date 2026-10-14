namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestStoreChecks
{
    public static void MemoryFloorPlanRequestStoreFindsWhatItSaved()
    {
        var store = FloorPlanRequestStore.NewMemoryFloorPlanRequestStore();
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
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
        MemoryFloorPlanRequestStoreFindsWhatItSaved();
    }
}
