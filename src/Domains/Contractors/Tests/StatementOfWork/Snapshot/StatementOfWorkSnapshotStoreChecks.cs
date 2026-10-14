namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkSnapshotStoreChecks
{
    public static void MemoryStatementOfWorkSnapshotStoreFindsWhatItSaved()
    {
        var store = StatementOfWorkSnapshotStore.NewMemoryStatementOfWorkSnapshotStore();
        var item = StatementOfWorkSnapshot.NewStatementOfWorkSnapshot("code-360", "label-601");
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
        MemoryStatementOfWorkSnapshotStoreFindsWhatItSaved();
    }
}
