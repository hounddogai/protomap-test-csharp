namespace Acme.Workforce.Compensation.Tests.Adjustment.Rollup;

using Acme.Workforce.Compensation.Adjustment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RegionalAdjustmentReservationStoreChecks
{
    public static void MemoryRegionalAdjustmentReservationStoreFindsWhatItSaved()
    {
        var store = RegionalAdjustmentReservationStore.NewMemoryRegionalAdjustmentReservationStore();
        var item = RegionalAdjustmentReservation.NewRegionalAdjustmentReservation("code-85", "title-363");
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
        MemoryRegionalAdjustmentReservationStoreFindsWhatItSaved();
    }
}
