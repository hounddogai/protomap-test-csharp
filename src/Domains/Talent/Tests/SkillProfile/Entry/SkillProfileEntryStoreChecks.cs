namespace Acme.Workforce.Talent.Tests.SkillProfile.Entry;

using Acme.Workforce.Talent.SkillProfile.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SkillProfileEntryStoreChecks
{
    public static void MemorySkillProfileEntryStoreFindsWhatItSaved()
    {
        var store = SkillProfileEntryStore.NewMemorySkillProfileEntryStore();
        var item = SkillProfileEntry.NewSkillProfileEntry("code-929", "label-613");
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
        MemorySkillProfileEntryStoreFindsWhatItSaved();
    }
}
