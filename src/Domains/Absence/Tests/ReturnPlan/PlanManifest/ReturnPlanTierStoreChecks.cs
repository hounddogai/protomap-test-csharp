namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class ReturnPlanTierStoreChecks
{
    public static void MemoryReturnPlanTierStoreFindsWhatItSaved()
    {
        var store = ReturnPlanTierStore.NewMemoryReturnPlanTierStore();
        var item = ReturnPlanTier.NewReturnPlanTier("code-807", "label-348");
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
        MemoryReturnPlanTierStoreFindsWhatItSaved();
    }
}
