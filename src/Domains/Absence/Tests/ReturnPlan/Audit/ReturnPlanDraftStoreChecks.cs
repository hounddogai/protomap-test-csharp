namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftStoreChecks
{
    public static void MemoryReturnPlanDraftStoreFindsWhatItSaved()
    {
        var store = ReturnPlanDraftStore.NewMemoryReturnPlanDraftStore();
        var item = ReturnPlanDraft.NewReturnPlanDraft("code-864", "label-671");
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
        MemoryReturnPlanDraftStoreFindsWhatItSaved();
    }
}
