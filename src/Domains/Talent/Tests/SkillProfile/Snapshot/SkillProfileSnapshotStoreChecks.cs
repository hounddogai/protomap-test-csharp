namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileSnapshotStoreChecks
{
    public static void MemorySkillProfileSnapshotStoreFindsWhatItSaved()
    {
        var store = SkillProfileSnapshotStore.NewMemorySkillProfileSnapshotStore();
        var item = SkillProfileSnapshot.NewSkillProfileSnapshot("code-496", "label-781");
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
        MemorySkillProfileSnapshotStoreFindsWhatItSaved();
    }
}
