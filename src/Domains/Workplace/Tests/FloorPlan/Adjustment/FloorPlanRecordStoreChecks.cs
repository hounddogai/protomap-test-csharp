namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordStoreChecks
{
    public static void MemoryFloorPlanRecordStoreFindsWhatItSaved()
    {
        var store = FloorPlanRecordStore.NewMemoryFloorPlanRecordStore();
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
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
        MemoryFloorPlanRecordStoreFindsWhatItSaved();
    }
}
