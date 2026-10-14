namespace Acme.Workforce.Engagement.Tests.Recognition.Milestone;

using Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledRecognitionBundleStoreChecks
{
    public static void MemoryScheduledRecognitionBundleStoreFindsWhatItSaved()
    {
        var store = ScheduledRecognitionBundleStore.NewMemoryScheduledRecognitionBundleStore();
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("code-319", "label-276");
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
        MemoryScheduledRecognitionBundleStoreFindsWhatItSaved();
    }
}
