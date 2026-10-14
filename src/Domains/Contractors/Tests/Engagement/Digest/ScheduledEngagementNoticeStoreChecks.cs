namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ScheduledEngagementNoticeStoreChecks
{
    public static void MemoryScheduledEngagementNoticeStoreFindsWhatItSaved()
    {
        var store = ScheduledEngagementNoticeStore.NewMemoryScheduledEngagementNoticeStore();
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("code-50", "name-64");
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
        MemoryScheduledEngagementNoticeStoreFindsWhatItSaved();
    }
}
