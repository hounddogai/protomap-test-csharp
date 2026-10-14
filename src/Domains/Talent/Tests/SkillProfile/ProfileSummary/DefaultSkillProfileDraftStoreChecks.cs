namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileSummary;

using Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>Checks of profile summary code, run by the project's runner.</summary>
public static class DefaultSkillProfileDraftStoreChecks
{
    public static void MemoryDefaultSkillProfileDraftStoreFindsWhatItSaved()
    {
        var store = DefaultSkillProfileDraftStore.NewMemoryDefaultSkillProfileDraftStore();
        var item = DefaultSkillProfileDraft.NewDefaultSkillProfileDraft("code-307", "name-985");
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
        MemoryDefaultSkillProfileDraftStoreFindsWhatItSaved();
    }
}
