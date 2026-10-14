namespace Acme.Workforce.Talent.Tests.SkillProfile.Target;

using Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacySkillProfileCheckpointStoreChecks
{
    public static void MemoryLegacySkillProfileCheckpointStoreFindsWhatItSaved()
    {
        var store = LegacySkillProfileCheckpointStore.NewMemoryLegacySkillProfileCheckpointStore();
        var item = LegacySkillProfileCheckpoint.NewLegacySkillProfileCheckpoint("code-696", "name-942");
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
        MemoryLegacySkillProfileCheckpointStoreFindsWhatItSaved();
    }
}
