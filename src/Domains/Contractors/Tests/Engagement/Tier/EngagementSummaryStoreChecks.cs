namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementSummaryStoreChecks
{
    public static void MemoryEngagementSummaryStoreFindsWhatItSaved()
    {
        var store = EngagementSummaryStore.NewMemoryEngagementSummaryStore();
        var item = EngagementSummary.NewEngagementSummary("code-595", "label-187");
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
        MemoryEngagementSummaryStoreFindsWhatItSaved();
    }
}
