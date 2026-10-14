namespace Acme.Workforce.Talent.Tests.Nomination.NominationReservation;

using Acme.Workforce.Talent.Nomination.NominationReservation;

/// <summary>Checks of nomination reservation code, run by the project's runner.</summary>
public static class BatchedNominationReservationStoreChecks
{
    public static void MemoryBatchedNominationReservationStoreFindsWhatItSaved()
    {
        var store = BatchedNominationReservationStore.NewMemoryBatchedNominationReservationStore();
        var item = BatchedNominationReservation.NewBatchedNominationReservation("code-847", "title-809");
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
        MemoryBatchedNominationReservationStoreFindsWhatItSaved();
    }
}
