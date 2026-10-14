namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class StrictFloorPlanBundleStoreChecks
{
    public static void MemoryStrictFloorPlanBundleStoreFindsWhatItSaved()
    {
        var store = StrictFloorPlanBundleStore.NewMemoryStrictFloorPlanBundleStore();
        var item = StrictFloorPlanBundle.NewStrictFloorPlanBundle("code-528", "name-424");
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
        MemoryStrictFloorPlanBundleStoreFindsWhatItSaved();
    }
}
