namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSuccessionPlanDraftStoreChecks
{
    public static void MemoryRegionalSuccessionPlanDraftStoreFindsWhatItSaved()
    {
        var store = RegionalSuccessionPlanDraftStore.NewMemoryRegionalSuccessionPlanDraftStore();
        var item = RegionalSuccessionPlanDraft.NewRegionalSuccessionPlanDraft("code-489", "title-653");
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
        MemoryRegionalSuccessionPlanDraftStoreFindsWhatItSaved();
    }
}
