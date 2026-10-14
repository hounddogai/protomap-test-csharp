namespace Acme.Workforce.Talent.Tests.Mobility.MobilityAudit;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Checks of mobility audit code, run by the project's runner.</summary>
public static class BatchedMobilityProfileStoreChecks
{
    public static void MemoryBatchedMobilityProfileStoreFindsWhatItSaved()
    {
        var store = BatchedMobilityProfileStore.NewMemoryBatchedMobilityProfileStore();
        var item = BatchedMobilityProfile.NewBatchedMobilityProfile("code-378", "label-973");
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
        MemoryBatchedMobilityProfileStoreFindsWhatItSaved();
    }
}
