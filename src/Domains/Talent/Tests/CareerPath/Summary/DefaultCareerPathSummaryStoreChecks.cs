namespace Acme.Workforce.Talent.Tests.CareerPath.Summary;

using Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultCareerPathSummaryStoreChecks
{
    public static void MemoryDefaultCareerPathSummaryStoreFindsWhatItSaved()
    {
        var store = DefaultCareerPathSummaryStore.NewMemoryDefaultCareerPathSummaryStore();
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("code-990", "title-666");
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
        MemoryDefaultCareerPathSummaryStoreFindsWhatItSaved();
    }
}
