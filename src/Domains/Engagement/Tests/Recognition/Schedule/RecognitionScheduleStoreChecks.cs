namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleStoreChecks
{
    public static void MemoryRecognitionScheduleStoreFindsWhatItSaved()
    {
        var store = RecognitionScheduleStore.NewMemoryRecognitionScheduleStore();
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
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
        MemoryRecognitionScheduleStoreFindsWhatItSaved();
    }
}
