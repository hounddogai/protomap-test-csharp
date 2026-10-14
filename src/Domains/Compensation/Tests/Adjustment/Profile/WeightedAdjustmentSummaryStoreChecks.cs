namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAdjustmentSummaryStoreChecks
{
    public static void MemoryWeightedAdjustmentSummaryStoreFindsWhatItSaved()
    {
        var store = WeightedAdjustmentSummaryStore.NewMemoryWeightedAdjustmentSummaryStore();
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
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
        MemoryWeightedAdjustmentSummaryStoreFindsWhatItSaved();
    }
}
