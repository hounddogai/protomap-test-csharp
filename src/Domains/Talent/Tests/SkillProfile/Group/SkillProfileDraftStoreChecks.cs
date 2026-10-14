namespace Acme.Workforce.Talent.Tests.SkillProfile.Group;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SkillProfileDraftStoreChecks
{
    public static void MemorySkillProfileDraftStoreFindsWhatItSaved()
    {
        var store = SkillProfileDraftStore.NewMemorySkillProfileDraftStore();
        var item = SkillProfileDraft.NewSkillProfileDraft("code-718", "name-811");
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
        MemorySkillProfileDraftStoreFindsWhatItSaved();
    }
}
