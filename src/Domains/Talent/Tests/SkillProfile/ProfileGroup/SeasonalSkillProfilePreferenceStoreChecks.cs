namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>Checks of profile group code, run by the project's runner.</summary>
public static class SeasonalSkillProfilePreferenceStoreChecks
{
    public static void MemorySeasonalSkillProfilePreferenceStoreFindsWhatItSaved()
    {
        var store = SeasonalSkillProfilePreferenceStore.NewMemorySeasonalSkillProfilePreferenceStore();
        var item = SeasonalSkillProfilePreference.NewSeasonalSkillProfilePreference("code-293", "title-427");
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
        MemorySeasonalSkillProfilePreferenceStoreFindsWhatItSaved();
    }
}
