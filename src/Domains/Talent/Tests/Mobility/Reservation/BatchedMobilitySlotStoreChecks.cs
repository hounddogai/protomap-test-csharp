namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedMobilitySlotStoreChecks
{
    public static void MemoryBatchedMobilitySlotStoreFindsWhatItSaved()
    {
        var store = BatchedMobilitySlotStore.NewMemoryBatchedMobilitySlotStore();
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("code-67", "title-523");
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
        MemoryBatchedMobilitySlotStoreFindsWhatItSaved();
    }
}
