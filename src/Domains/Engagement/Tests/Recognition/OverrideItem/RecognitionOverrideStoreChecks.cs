namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionOverrideStoreChecks
{
    public static void MemoryRecognitionOverrideStoreFindsWhatItSaved()
    {
        var store = RecognitionOverrideStore.NewMemoryRecognitionOverrideStore();
        var item = RecognitionOverride.NewRecognitionOverride("code-725", "label-571");
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
        MemoryRecognitionOverrideStoreFindsWhatItSaved();
    }
}
