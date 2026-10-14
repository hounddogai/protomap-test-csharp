namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanPreference;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Checks of plan preference code, run by the project's runner.</summary>
public static class DefaultActionPlanPreferenceStoreChecks
{
    public static void MemoryDefaultActionPlanPreferenceStoreFindsWhatItSaved()
    {
        var store = DefaultActionPlanPreferenceStore.NewMemoryDefaultActionPlanPreferenceStore();
        var item = DefaultActionPlanPreference.NewDefaultActionPlanPreference("code-206", "name-62");
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
        MemoryDefaultActionPlanPreferenceStoreFindsWhatItSaved();
    }
}
