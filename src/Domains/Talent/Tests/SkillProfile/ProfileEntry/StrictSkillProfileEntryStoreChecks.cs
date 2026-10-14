namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileEntry;

using Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>Checks of profile entry code, run by the project's runner.</summary>
public static class StrictSkillProfileEntryStoreChecks
{
    public static void MemoryStrictSkillProfileEntryStoreFindsWhatItSaved()
    {
        var store = StrictSkillProfileEntryStore.NewMemoryStrictSkillProfileEntryStore();
        var item = StrictSkillProfileEntry.NewStrictSkillProfileEntry("code-382", "title-180");
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
        MemoryStrictSkillProfileEntryStoreFindsWhatItSaved();
    }
}
