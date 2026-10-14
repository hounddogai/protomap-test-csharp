namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledAdjustmentReservationStoreChecks
{
    public static void MemoryScheduledAdjustmentReservationStoreFindsWhatItSaved()
    {
        var store = ScheduledAdjustmentReservationStore.NewMemoryScheduledAdjustmentReservationStore();
        var item = ScheduledAdjustmentReservation.NewScheduledAdjustmentReservation("code-651", "title-532");
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
        MemoryScheduledAdjustmentReservationStoreFindsWhatItSaved();
    }
}
