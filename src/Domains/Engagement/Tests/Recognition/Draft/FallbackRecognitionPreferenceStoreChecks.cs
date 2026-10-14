namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class FallbackRecognitionPreferenceStoreChecks
{
    public static void MemoryFallbackRecognitionPreferenceStoreFindsWhatItSaved()
    {
        var store = FallbackRecognitionPreferenceStore.NewMemoryFallbackRecognitionPreferenceStore();
        var item = FallbackRecognitionPreference.NewFallbackRecognitionPreference("code-654", "name-317");
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
        MemoryFallbackRecognitionPreferenceStoreFindsWhatItSaved();
    }
}
