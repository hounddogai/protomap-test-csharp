namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentSlotStoreChecks
{
    public static void MemoryAdjustmentSlotStoreFindsWhatItSaved()
    {
        var store = AdjustmentSlotStore.NewMemoryAdjustmentSlotStore();
        var item = AdjustmentSlot.NewAdjustmentSlot("code-783", "title-756");
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
        MemoryAdjustmentSlotStoreFindsWhatItSaved();
    }
}
