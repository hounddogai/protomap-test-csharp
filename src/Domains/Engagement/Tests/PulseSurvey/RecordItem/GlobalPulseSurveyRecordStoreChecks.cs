namespace Acme.Workforce.Engagement.Tests.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalPulseSurveyRecordStoreChecks
{
    public static void MemoryGlobalPulseSurveyRecordStoreFindsWhatItSaved()
    {
        var store = GlobalPulseSurveyRecordStore.NewMemoryGlobalPulseSurveyRecordStore();
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
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
        MemoryGlobalPulseSurveyRecordStoreFindsWhatItSaved();
    }
}
