namespace Acme.Workforce.Engagement.Tests.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

/// <summary>Checks of regional plan preference code, run by the project's runner.</summary>
public static class ActionPlanVarianceStoreChecks
{
    public static void MemoryActionPlanVarianceStoreFindsWhatItSaved()
    {
        var store = ActionPlanVarianceStore.NewMemoryActionPlanVarianceStore();
        var item = ActionPlanVariance.NewActionPlanVariance("code-227", "label-407");
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
        MemoryActionPlanVarianceStoreFindsWhatItSaved();
    }
}
