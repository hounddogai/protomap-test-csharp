namespace Acme.Workforce.Talent.Tests.SkillProfile.ProfileProposal;

using Acme.Workforce.Talent.SkillProfile.ProfileProposal;

/// <summary>Checks of profile proposal code, run by the project's runner.</summary>
public static class SkillProfileSummaryStoreChecks
{
    public static void MemorySkillProfileSummaryStoreFindsWhatItSaved()
    {
        var store = SkillProfileSummaryStore.NewMemorySkillProfileSummaryStore();
        var item = SkillProfileSummary.NewSkillProfileSummary("code-994", "name-547");
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
        MemorySkillProfileSummaryStoreFindsWhatItSaved();
    }
}
