namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestStoreChecks
{
    public static void MemoryFallbackFloorPlanRequestStoreFindsWhatItSaved()
    {
        var store = FallbackFloorPlanRequestStore.NewMemoryFallbackFloorPlanRequestStore();
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
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
        MemoryFallbackFloorPlanRequestStoreFindsWhatItSaved();
    }
}
