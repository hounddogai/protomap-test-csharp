namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacySkillProfilePreferenceStoreChecks
{
    public static void MemoryLegacySkillProfilePreferenceStoreFindsWhatItSaved()
    {
        var store = LegacySkillProfilePreferenceStore.NewMemoryLegacySkillProfilePreferenceStore();
        var item = LegacySkillProfilePreference.NewLegacySkillProfilePreference("code-704", "name-17");
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
        MemoryLegacySkillProfilePreferenceStoreFindsWhatItSaved();
    }
}
