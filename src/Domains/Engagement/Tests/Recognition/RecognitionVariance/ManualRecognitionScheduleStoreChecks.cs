namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class ManualRecognitionScheduleStoreChecks
{
    public static void MemoryManualRecognitionScheduleStoreFindsWhatItSaved()
    {
        var store = ManualRecognitionScheduleStore.NewMemoryManualRecognitionScheduleStore();
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("code-325", "title-418");
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
        MemoryManualRecognitionScheduleStoreFindsWhatItSaved();
    }
}
