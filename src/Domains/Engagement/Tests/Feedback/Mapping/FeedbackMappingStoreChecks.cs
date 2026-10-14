namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FeedbackMappingStoreChecks
{
    public static void MemoryFeedbackMappingStoreFindsWhatItSaved()
    {
        var store = FeedbackMappingStore.NewMemoryFeedbackMappingStore();
        var item = FeedbackMapping.NewFeedbackMapping("code-542", "title-91");
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
        MemoryFeedbackMappingStoreFindsWhatItSaved();
    }
}
