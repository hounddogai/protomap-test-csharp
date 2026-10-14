namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalAdjustmentRollupStoreChecks
{
    public static void MemoryGlobalAdjustmentRollupStoreFindsWhatItSaved()
    {
        var store = GlobalAdjustmentRollupStore.NewMemoryGlobalAdjustmentRollupStore();
        var item = GlobalAdjustmentRollup.NewGlobalAdjustmentRollup("code-576", "name-767");
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
        MemoryGlobalAdjustmentRollupStoreFindsWhatItSaved();
    }
}
