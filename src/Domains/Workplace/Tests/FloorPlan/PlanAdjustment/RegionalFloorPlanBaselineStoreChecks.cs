namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class RegionalFloorPlanBaselineStoreChecks
{
    public static void MemoryRegionalFloorPlanBaselineStoreFindsWhatItSaved()
    {
        var store = RegionalFloorPlanBaselineStore.NewMemoryRegionalFloorPlanBaselineStore();
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("code-906", "label-553");
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
        MemoryRegionalFloorPlanBaselineStoreFindsWhatItSaved();
    }
}
