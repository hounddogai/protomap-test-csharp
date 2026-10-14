namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryFloorPlanNoticeStoreChecks
{
    public static void MemoryPrimaryFloorPlanNoticeStoreFindsWhatItSaved()
    {
        var store = PrimaryFloorPlanNoticeStore.NewMemoryPrimaryFloorPlanNoticeStore();
        var item = PrimaryFloorPlanNotice.NewPrimaryFloorPlanNotice("code-569", "label-686");
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
        MemoryPrimaryFloorPlanNoticeStoreFindsWhatItSaved();
    }
}
