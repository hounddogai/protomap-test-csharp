namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionProfileStoreChecks
{
    public static void MemoryRecognitionProfileStoreFindsWhatItSaved()
    {
        var store = RecognitionProfileStore.NewMemoryRecognitionProfileStore();
        var item = RecognitionProfile.NewRecognitionProfile("code-771", "label-29");
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
        MemoryRecognitionProfileStoreFindsWhatItSaved();
    }
}
