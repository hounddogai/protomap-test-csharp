namespace Acme.Workforce.Absence.Tests.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

/// <summary>Checks of seasonal plan manifest code, run by the project's runner.</summary>
public static class RegionalReturnPlanItemStoreChecks
{
    public static void MemoryRegionalReturnPlanItemStoreFindsWhatItSaved()
    {
        var store = RegionalReturnPlanItemStore.NewMemoryRegionalReturnPlanItemStore();
        var item = RegionalReturnPlanItem.NewRegionalReturnPlanItem("code-523", "name-239");
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
        MemoryRegionalReturnPlanItemStoreFindsWhatItSaved();
    }
}
