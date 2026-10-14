namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PendingFeedbackMappingStoreChecks
{
    public static void MemoryPendingFeedbackMappingStoreFindsWhatItSaved()
    {
        var store = PendingFeedbackMappingStore.NewMemoryPendingFeedbackMappingStore();
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("code-571", "name-791");
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
        MemoryPendingFeedbackMappingStoreFindsWhatItSaved();
    }
}
