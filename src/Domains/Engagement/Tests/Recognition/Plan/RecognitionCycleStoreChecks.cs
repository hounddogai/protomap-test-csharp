namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionCycleStoreChecks
{
    public static void MemoryRecognitionCycleStoreFindsWhatItSaved()
    {
        var store = RecognitionCycleStore.NewMemoryRecognitionCycleStore();
        var item = RecognitionCycle.NewRecognitionCycle("code-384", "name-558");
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
        MemoryRecognitionCycleStoreFindsWhatItSaved();
    }
}
