namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanPreferenceStoreChecks
{
    public static void MemoryReturnPlanPreferenceStoreFindsWhatItSaved()
    {
        var store = ReturnPlanPreferenceStore.NewMemoryReturnPlanPreferenceStore();
        var item = ReturnPlanPreference.NewReturnPlanPreference("code-652", "title-590");
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
        MemoryReturnPlanPreferenceStoreFindsWhatItSaved();
    }
}
