namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class PrimaryEngagementNoticeStoreChecks
{
    public static void MemoryPrimaryEngagementNoticeStoreFindsWhatItSaved()
    {
        var store = PrimaryEngagementNoticeStore.NewMemoryPrimaryEngagementNoticeStore();
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("code-642", "label-919");
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
        MemoryPrimaryEngagementNoticeStoreFindsWhatItSaved();
    }
}
