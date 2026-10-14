namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationStoreChecks
{
    public static void MemoryDefaultBonusPoolReservationStoreFindsWhatItSaved()
    {
        var store = DefaultBonusPoolReservationStore.NewMemoryDefaultBonusPoolReservationStore();
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
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
        MemoryDefaultBonusPoolReservationStoreFindsWhatItSaved();
    }
}
