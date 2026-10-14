namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionRequestStoreChecks
{
    public static void MemoryRecognitionRequestStoreFindsWhatItSaved()
    {
        var store = RecognitionRequestStore.NewMemoryRecognitionRequestStore();
        var item = RecognitionRequest.NewRecognitionRequest("code-450", "title-285");
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
        MemoryRecognitionRequestStoreFindsWhatItSaved();
    }
}
