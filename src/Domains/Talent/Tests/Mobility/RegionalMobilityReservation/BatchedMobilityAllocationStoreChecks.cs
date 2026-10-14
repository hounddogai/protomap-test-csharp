namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class BatchedMobilityAllocationStoreChecks
{
    public static void MemoryBatchedMobilityAllocationStoreFindsWhatItSaved()
    {
        var store = BatchedMobilityAllocationStore.NewMemoryBatchedMobilityAllocationStore();
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
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
        MemoryBatchedMobilityAllocationStoreFindsWhatItSaved();
    }
}
