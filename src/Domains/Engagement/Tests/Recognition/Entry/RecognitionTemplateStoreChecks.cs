namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionTemplateStoreChecks
{
    public static void MemoryRecognitionTemplateStoreFindsWhatItSaved()
    {
        var store = RecognitionTemplateStore.NewMemoryRecognitionTemplateStore();
        var item = RecognitionTemplate.NewRecognitionTemplate("code-894", "name-483");
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
        MemoryRecognitionTemplateStoreFindsWhatItSaved();
    }
}
