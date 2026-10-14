namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedFloorPlanThresholdStoreChecks
{
    public static void MemoryCappedFloorPlanThresholdStoreFindsWhatItSaved()
    {
        var store = CappedFloorPlanThresholdStore.NewMemoryCappedFloorPlanThresholdStore();
        var item = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold("code-898", "label-846");
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
        MemoryCappedFloorPlanThresholdStoreFindsWhatItSaved();
    }
}
