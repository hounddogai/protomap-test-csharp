namespace Acme.Workforce.Talent.Tests.SkillProfile.Threshold;

using Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RollingSkillProfileRecordStoreChecks
{
    public static void MemoryRollingSkillProfileRecordStoreFindsWhatItSaved()
    {
        var store = RollingSkillProfileRecordStore.NewMemoryRollingSkillProfileRecordStore();
        var item = RollingSkillProfileRecord.NewRollingSkillProfileRecord("code-256", "name-927");
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
        MemoryRollingSkillProfileRecordStoreFindsWhatItSaved();
    }
}
