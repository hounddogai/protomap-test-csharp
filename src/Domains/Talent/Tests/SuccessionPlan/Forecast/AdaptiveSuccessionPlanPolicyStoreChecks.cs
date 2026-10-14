namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AdaptiveSuccessionPlanPolicyStoreChecks
{
    public static void MemoryAdaptiveSuccessionPlanPolicyStoreFindsWhatItSaved()
    {
        var store = AdaptiveSuccessionPlanPolicyStore.NewMemoryAdaptiveSuccessionPlanPolicyStore();
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("code-832", "name-493");
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
        MemoryAdaptiveSuccessionPlanPolicyStoreFindsWhatItSaved();
    }
}
