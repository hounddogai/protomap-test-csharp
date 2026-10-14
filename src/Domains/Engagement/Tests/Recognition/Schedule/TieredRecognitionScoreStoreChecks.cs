namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredRecognitionScoreStoreChecks
{
    public static void MemoryTieredRecognitionScoreStoreFindsWhatItSaved()
    {
        var store = TieredRecognitionScoreStore.NewMemoryTieredRecognitionScoreStore();
        var item = TieredRecognitionScore.NewTieredRecognitionScore("code-61", "name-198");
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
        MemoryTieredRecognitionScoreStoreFindsWhatItSaved();
    }
}
