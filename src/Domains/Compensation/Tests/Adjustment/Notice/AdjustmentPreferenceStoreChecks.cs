namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceStoreChecks
{
    public static void MemoryAdjustmentPreferenceStoreFindsWhatItSaved()
    {
        var store = AdjustmentPreferenceStore.NewMemoryAdjustmentPreferenceStore();
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
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
        MemoryAdjustmentPreferenceStoreFindsWhatItSaved();
    }
}
