namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathSummaryStoreChecks
{
    public static void MemoryRegionalCareerPathSummaryStoreFindsWhatItSaved()
    {
        var store = RegionalCareerPathSummaryStore.NewMemoryRegionalCareerPathSummaryStore();
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("code-498", "title-732");
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
        MemoryRegionalCareerPathSummaryStoreFindsWhatItSaved();
    }
}
