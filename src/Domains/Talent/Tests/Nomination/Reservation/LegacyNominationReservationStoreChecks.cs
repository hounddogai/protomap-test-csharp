namespace Acme.Workforce.Talent.Tests.Nomination.Reservation;

using Acme.Workforce.Talent.Nomination.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyNominationReservationStoreChecks
{
    public static void MemoryLegacyNominationReservationStoreFindsWhatItSaved()
    {
        var store = LegacyNominationReservationStore.NewMemoryLegacyNominationReservationStore();
        var item = LegacyNominationReservation.NewLegacyNominationReservation("code-708", "label-899");
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
        MemoryLegacyNominationReservationStoreFindsWhatItSaved();
    }
}
