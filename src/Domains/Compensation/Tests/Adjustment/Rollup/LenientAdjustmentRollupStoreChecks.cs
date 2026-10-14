namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientAdjustmentRollupStoreChecks
{
    public static void MemoryLenientAdjustmentRollupStoreFindsWhatItSaved()
    {
        var store = LenientAdjustmentRollupStore.NewMemoryLenientAdjustmentRollupStore();
        var item = LenientAdjustmentRollup.NewLenientAdjustmentRollup("code-735", "title-435");
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
        MemoryLenientAdjustmentRollupStoreFindsWhatItSaved();
    }
}
