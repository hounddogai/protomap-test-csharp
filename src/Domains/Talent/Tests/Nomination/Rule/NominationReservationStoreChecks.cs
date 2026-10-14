namespace Acme.Workforce.Talent.Tests.Nomination.Rule;

using Acme.Workforce.Talent.Nomination.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class NominationReservationStoreChecks
{
    public static void MemoryNominationReservationStoreFindsWhatItSaved()
    {
        var store = NominationReservationStore.NewMemoryNominationReservationStore();
        var item = NominationReservation.NewNominationReservation("code-762", "label-897");
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
        MemoryNominationReservationStoreFindsWhatItSaved();
    }
}
