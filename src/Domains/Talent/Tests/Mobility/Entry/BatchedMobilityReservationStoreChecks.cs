namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedMobilityReservationStoreChecks
{
    public static void MemoryBatchedMobilityReservationStoreFindsWhatItSaved()
    {
        var store = BatchedMobilityReservationStore.NewMemoryBatchedMobilityReservationStore();
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
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
        MemoryBatchedMobilityReservationStoreFindsWhatItSaved();
    }
}
