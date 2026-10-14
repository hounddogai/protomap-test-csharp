namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionBundleStoreChecks
{
    public static void MemoryRecognitionBundleStoreFindsWhatItSaved()
    {
        var store = RecognitionBundleStore.NewMemoryRecognitionBundleStore();
        var item = RecognitionBundle.NewRecognitionBundle("code-813", "name-920");
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
        MemoryRecognitionBundleStoreFindsWhatItSaved();
    }
}
