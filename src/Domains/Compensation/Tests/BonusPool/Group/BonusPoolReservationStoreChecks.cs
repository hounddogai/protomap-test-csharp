namespace Acme.Workforce.Compensation.Tests.BonusPool.Group;

using Acme.Workforce.Compensation.BonusPool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BonusPoolReservationStoreChecks
{
    public static void MemoryBonusPoolReservationStoreFindsWhatItSaved()
    {
        var store = BonusPoolReservationStore.NewMemoryBonusPoolReservationStore();
        var item = BonusPoolReservation.NewBonusPoolReservation("code-184", "label-634");
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
        MemoryBonusPoolReservationStoreFindsWhatItSaved();
    }
}
