namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionSlotStoreChecks
{
    public static void MemoryRecognitionSlotStoreFindsWhatItSaved()
    {
        var store = RecognitionSlotStore.NewMemoryRecognitionSlotStore();
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
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
        MemoryRecognitionSlotStoreFindsWhatItSaved();
    }
}
