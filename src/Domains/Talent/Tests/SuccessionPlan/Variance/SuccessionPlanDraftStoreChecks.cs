namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanDraftStoreChecks
{
    public static void MemorySuccessionPlanDraftStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanDraftStore.NewMemorySuccessionPlanDraftStore();
        var item = SuccessionPlanDraft.NewSuccessionPlanDraft("code-326", "title-862");
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
        MemorySuccessionPlanDraftStoreFindsWhatItSaved();
    }
}
