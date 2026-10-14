namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentSnapshotStoreChecks
{
    public static void MemoryAssignmentSnapshotStoreFindsWhatItSaved()
    {
        var store = AssignmentSnapshotStore.NewMemoryAssignmentSnapshotStore();
        var item = AssignmentSnapshot.NewAssignmentSnapshot("code-559", "name-697");
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
        MemoryAssignmentSnapshotStoreFindsWhatItSaved();
    }
}
