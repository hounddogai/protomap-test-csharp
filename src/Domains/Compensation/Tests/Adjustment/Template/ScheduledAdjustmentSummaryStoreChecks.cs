namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAdjustmentSummaryStoreChecks
{
    public static void MemoryScheduledAdjustmentSummaryStoreFindsWhatItSaved()
    {
        var store = ScheduledAdjustmentSummaryStore.NewMemoryScheduledAdjustmentSummaryStore();
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("code-664", "label-280");
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
        MemoryScheduledAdjustmentSummaryStoreFindsWhatItSaved();
    }
}
