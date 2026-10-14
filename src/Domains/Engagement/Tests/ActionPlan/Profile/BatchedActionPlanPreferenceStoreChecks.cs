namespace Acme.Workforce.Engagement.Tests.ActionPlan.Profile;

using Acme.Workforce.Engagement.ActionPlan.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BatchedActionPlanPreferenceStoreChecks
{
    public static void MemoryBatchedActionPlanPreferenceStoreFindsWhatItSaved()
    {
        var store = BatchedActionPlanPreferenceStore.NewMemoryBatchedActionPlanPreferenceStore();
        var item = BatchedActionPlanPreference.NewBatchedActionPlanPreference("code-745", "label-844");
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
        MemoryBatchedActionPlanPreferenceStoreFindsWhatItSaved();
    }
}
