namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentReservationStoreChecks
{
    public static void MemoryAdjustmentReservationStoreFindsWhatItSaved()
    {
        var store = AdjustmentReservationStore.NewMemoryAdjustmentReservationStore();
        var item = AdjustmentReservation.NewAdjustmentReservation("code-198", "name-755");
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
        MemoryAdjustmentReservationStoreFindsWhatItSaved();
    }
}
