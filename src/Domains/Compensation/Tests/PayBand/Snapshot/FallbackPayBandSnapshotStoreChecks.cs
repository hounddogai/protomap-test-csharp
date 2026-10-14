namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackPayBandSnapshotStoreChecks
{
    public static void MemoryFallbackPayBandSnapshotStoreFindsWhatItSaved()
    {
        var store = FallbackPayBandSnapshotStore.NewMemoryFallbackPayBandSnapshotStore();
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("code-520", "title-3");
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
        MemoryFallbackPayBandSnapshotStoreFindsWhatItSaved();
    }
}
