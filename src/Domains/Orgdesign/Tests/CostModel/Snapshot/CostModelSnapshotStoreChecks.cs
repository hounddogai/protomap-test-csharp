namespace Acme.Workforce.Orgdesign.Tests.CostModel.Snapshot;

using Acme.Workforce.Orgdesign.CostModel.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class CostModelSnapshotStoreChecks
{
    public static void MemoryCostModelSnapshotStoreFindsWhatItSaved()
    {
        var store = CostModelSnapshotStore.NewMemoryCostModelSnapshotStore();
        var item = CostModelSnapshot.NewCostModelSnapshot("code-978", "title-224");
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
        MemoryCostModelSnapshotStoreFindsWhatItSaved();
    }
}
