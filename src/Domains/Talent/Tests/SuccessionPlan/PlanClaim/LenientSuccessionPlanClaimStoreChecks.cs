namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanClaimStoreChecks
{
    public static void MemoryLenientSuccessionPlanClaimStoreFindsWhatItSaved()
    {
        var store = LenientSuccessionPlanClaimStore.NewMemoryLenientSuccessionPlanClaimStore();
        var item = LenientSuccessionPlanClaim.NewLenientSuccessionPlanClaim("code-597", "label-734");
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
        MemoryLenientSuccessionPlanClaimStoreFindsWhatItSaved();
    }
}
