namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class WeightedAdjustmentDraftStoreChecks
{
    public static void MemoryWeightedAdjustmentDraftStoreFindsWhatItSaved()
    {
        var store = WeightedAdjustmentDraftStore.NewMemoryWeightedAdjustmentDraftStore();
        var item = WeightedAdjustmentDraft.NewWeightedAdjustmentDraft("code-912", "label-478");
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
        MemoryWeightedAdjustmentDraftStoreFindsWhatItSaved();
    }
}
