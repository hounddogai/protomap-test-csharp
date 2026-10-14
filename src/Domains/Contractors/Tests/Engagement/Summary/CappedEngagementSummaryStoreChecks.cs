namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedEngagementSummaryStoreChecks
{
    public static void MemoryCappedEngagementSummaryStoreFindsWhatItSaved()
    {
        var store = CappedEngagementSummaryStore.NewMemoryCappedEngagementSummaryStore();
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
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
        MemoryCappedEngagementSummaryStoreFindsWhatItSaved();
    }
}
