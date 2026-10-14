namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotStoreChecks
{
    public static void MemoryGlobalNominationSnapshotStoreFindsWhatItSaved()
    {
        var store = GlobalNominationSnapshotStore.NewMemoryGlobalNominationSnapshotStore();
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
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
        MemoryGlobalNominationSnapshotStoreFindsWhatItSaved();
    }
}
