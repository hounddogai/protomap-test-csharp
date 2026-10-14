namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanSummaryStoreChecks
{
    public static void MemorySuccessionPlanSummaryStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanSummaryStore.NewMemorySuccessionPlanSummaryStore();
        var item = SuccessionPlanSummary.NewSuccessionPlanSummary("code-182", "label-366");
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
        MemorySuccessionPlanSummaryStoreFindsWhatItSaved();
    }
}
