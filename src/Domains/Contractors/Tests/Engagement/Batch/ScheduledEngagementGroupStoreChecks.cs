namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledEngagementGroupStoreChecks
{
    public static void MemoryScheduledEngagementGroupStoreFindsWhatItSaved()
    {
        var store = ScheduledEngagementGroupStore.NewMemoryScheduledEngagementGroupStore();
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("code-562", "name-284");
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
        MemoryScheduledEngagementGroupStoreFindsWhatItSaved();
    }
}
