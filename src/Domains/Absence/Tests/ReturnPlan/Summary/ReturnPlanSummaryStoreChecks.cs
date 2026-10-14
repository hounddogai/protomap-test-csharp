namespace Acme.Workforce.Absence.Tests.ReturnPlan.Summary;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReturnPlanSummaryStoreChecks
{
    public static void MemoryReturnPlanSummaryStoreFindsWhatItSaved()
    {
        var store = ReturnPlanSummaryStore.NewMemoryReturnPlanSummaryStore();
        var item = ReturnPlanSummary.NewReturnPlanSummary("code-389", "name-184");
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
        MemoryReturnPlanSummaryStoreFindsWhatItSaved();
    }
}
