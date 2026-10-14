namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPayBandSnapshotStoreChecks
{
    public static void MemoryScheduledPayBandSnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledPayBandSnapshotStore.NewMemoryScheduledPayBandSnapshotStore();
        var item = ScheduledPayBandSnapshot.NewScheduledPayBandSnapshot("code-547", "name-663");
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
        MemoryScheduledPayBandSnapshotStoreFindsWhatItSaved();
    }
}
