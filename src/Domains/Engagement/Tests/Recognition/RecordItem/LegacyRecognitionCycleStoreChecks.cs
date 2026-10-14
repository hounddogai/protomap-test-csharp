namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRecognitionCycleStoreChecks
{
    public static void MemoryLegacyRecognitionCycleStoreFindsWhatItSaved()
    {
        var store = LegacyRecognitionCycleStore.NewMemoryLegacyRecognitionCycleStore();
        var item = LegacyRecognitionCycle.NewLegacyRecognitionCycle("code-545", "title-615");
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
        MemoryLegacyRecognitionCycleStoreFindsWhatItSaved();
    }
}
