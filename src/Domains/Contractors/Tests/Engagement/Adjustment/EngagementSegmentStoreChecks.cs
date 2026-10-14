namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementSegmentStoreChecks
{
    public static void MemoryEngagementSegmentStoreFindsWhatItSaved()
    {
        var store = EngagementSegmentStore.NewMemoryEngagementSegmentStore();
        var item = EngagementSegment.NewEngagementSegment("code-241", "title-193");
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
        MemoryEngagementSegmentStoreFindsWhatItSaved();
    }
}
