namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryAdjustmentPreferenceStoreChecks
{
    public static void MemoryPrimaryAdjustmentPreferenceStoreFindsWhatItSaved()
    {
        var store = PrimaryAdjustmentPreferenceStore.NewMemoryPrimaryAdjustmentPreferenceStore();
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
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
        MemoryPrimaryAdjustmentPreferenceStoreFindsWhatItSaved();
    }
}
