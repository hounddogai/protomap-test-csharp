namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualBonusPoolReservationStoreChecks
{
    public static void MemoryManualBonusPoolReservationStoreFindsWhatItSaved()
    {
        var store = ManualBonusPoolReservationStore.NewMemoryManualBonusPoolReservationStore();
        var item = ManualBonusPoolReservation.NewManualBonusPoolReservation("code-100", "label-683");
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
        MemoryManualBonusPoolReservationStoreFindsWhatItSaved();
    }
}
