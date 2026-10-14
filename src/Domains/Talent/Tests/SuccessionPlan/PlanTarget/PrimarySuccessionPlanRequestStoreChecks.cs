namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRequestStoreChecks
{
    public static void MemoryPrimarySuccessionPlanRequestStoreFindsWhatItSaved()
    {
        var store = PrimarySuccessionPlanRequestStore.NewMemoryPrimarySuccessionPlanRequestStore();
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("code-458", "title-701");
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
        MemoryPrimarySuccessionPlanRequestStoreFindsWhatItSaved();
    }
}
