namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanPreferenceStoreChecks
{
    public static void MemoryActionPlanPreferenceStoreFindsWhatItSaved()
    {
        var store = ActionPlanPreferenceStore.NewMemoryActionPlanPreferenceStore();
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
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
        MemoryActionPlanPreferenceStoreFindsWhatItSaved();
    }
}
