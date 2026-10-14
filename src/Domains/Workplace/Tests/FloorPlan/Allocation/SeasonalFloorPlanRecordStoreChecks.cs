namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordStoreChecks
{
    public static void MemorySeasonalFloorPlanRecordStoreFindsWhatItSaved()
    {
        var store = SeasonalFloorPlanRecordStore.NewMemorySeasonalFloorPlanRecordStore();
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
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
        MemorySeasonalFloorPlanRecordStoreFindsWhatItSaved();
    }
}
