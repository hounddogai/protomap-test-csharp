namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionReviewStoreChecks
{
    public static void MemoryRecognitionReviewStoreFindsWhatItSaved()
    {
        var store = RecognitionReviewStore.NewMemoryRecognitionReviewStore();
        var item = RecognitionReview.NewRecognitionReview("code-230", "title-310");
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
        MemoryRecognitionReviewStoreFindsWhatItSaved();
    }
}
