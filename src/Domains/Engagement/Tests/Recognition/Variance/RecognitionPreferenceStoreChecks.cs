namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionPreferenceStoreChecks
{
    public static void MemoryRecognitionPreferenceStoreFindsWhatItSaved()
    {
        var store = RecognitionPreferenceStore.NewMemoryRecognitionPreferenceStore();
        var item = RecognitionPreference.NewRecognitionPreference("code-961", "label-235");
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
        MemoryRecognitionPreferenceStoreFindsWhatItSaved();
    }
}
