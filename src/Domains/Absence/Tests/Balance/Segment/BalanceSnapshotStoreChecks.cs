namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceSnapshotStoreChecks
{
    public static void MemoryBalanceSnapshotStoreFindsWhatItSaved()
    {
        var store = BalanceSnapshotStore.NewMemoryBalanceSnapshotStore();
        var item = BalanceSnapshot.NewBalanceSnapshot("code-151", "label-965");
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
        MemoryBalanceSnapshotStoreFindsWhatItSaved();
    }
}
