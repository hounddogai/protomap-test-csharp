namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorSnapshotStoreChecks
{
    public static void MemoryVisitorSnapshotStoreFindsWhatItSaved()
    {
        var store = VisitorSnapshotStore.NewMemoryVisitorSnapshotStore();
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
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
        MemoryVisitorSnapshotStoreFindsWhatItSaved();
    }
}
