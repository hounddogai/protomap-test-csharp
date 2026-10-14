namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class EngagementRequestStoreChecks
{
    public static void MemoryEngagementRequestStoreFindsWhatItSaved()
    {
        var store = EngagementRequestStore.NewMemoryEngagementRequestStore();
        var item = EngagementRequest.NewEngagementRequest("code-732", "label-855");
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
        MemoryEngagementRequestStoreFindsWhatItSaved();
    }
}
