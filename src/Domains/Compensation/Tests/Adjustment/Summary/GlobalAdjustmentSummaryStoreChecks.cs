namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryStoreChecks
{
    public static void MemoryGlobalAdjustmentSummaryStoreFindsWhatItSaved()
    {
        var store = GlobalAdjustmentSummaryStore.NewMemoryGlobalAdjustmentSummaryStore();
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
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
        MemoryGlobalAdjustmentSummaryStoreFindsWhatItSaved();
    }
}
