namespace Acme.Workforce.Talent.Tests.SkillProfile.Summary;

using Acme.Workforce.Talent.SkillProfile.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AdaptiveSkillProfileSummaryStoreChecks
{
    public static void MemoryAdaptiveSkillProfileSummaryStoreFindsWhatItSaved()
    {
        var store = AdaptiveSkillProfileSummaryStore.NewMemoryAdaptiveSkillProfileSummaryStore();
        var item = AdaptiveSkillProfileSummary.NewAdaptiveSkillProfileSummary("code-340", "name-212");
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
        MemoryAdaptiveSkillProfileSummaryStoreFindsWhatItSaved();
    }
}
