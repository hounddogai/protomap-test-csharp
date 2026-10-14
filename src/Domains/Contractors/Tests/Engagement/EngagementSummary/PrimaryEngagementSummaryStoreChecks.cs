namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryStoreChecks
{
    public static void MemoryPrimaryEngagementSummaryStoreFindsWhatItSaved()
    {
        var store = PrimaryEngagementSummaryStore.NewMemoryPrimaryEngagementSummaryStore();
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
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
        MemoryPrimaryEngagementSummaryStoreFindsWhatItSaved();
    }
}
