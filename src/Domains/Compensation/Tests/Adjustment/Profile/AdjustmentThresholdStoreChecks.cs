namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentThresholdStoreChecks
{
    public static void MemoryAdjustmentThresholdStoreFindsWhatItSaved()
    {
        var store = AdjustmentThresholdStore.NewMemoryAdjustmentThresholdStore();
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
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
        MemoryAdjustmentThresholdStoreFindsWhatItSaved();
    }
}
