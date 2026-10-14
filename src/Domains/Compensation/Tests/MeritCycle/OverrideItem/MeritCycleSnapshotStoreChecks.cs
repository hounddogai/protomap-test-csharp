namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleSnapshotStoreChecks
{
    public static void MemoryMeritCycleSnapshotStoreFindsWhatItSaved()
    {
        var store = MeritCycleSnapshotStore.NewMemoryMeritCycleSnapshotStore();
        var item = MeritCycleSnapshot.NewMeritCycleSnapshot("code-799", "label-457");
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
        MemoryMeritCycleSnapshotStoreFindsWhatItSaved();
    }
}
