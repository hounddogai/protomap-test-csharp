namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualFloorPlanReservationStoreChecks
{
    public static void MemoryManualFloorPlanReservationStoreFindsWhatItSaved()
    {
        var store = ManualFloorPlanReservationStore.NewMemoryManualFloorPlanReservationStore();
        var item = ManualFloorPlanReservation.NewManualFloorPlanReservation("code-741", "name-666");
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
        MemoryManualFloorPlanReservationStoreFindsWhatItSaved();
    }
}
