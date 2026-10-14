namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordStoreChecks
{
    public static void MemoryPulseSurveyRecordStoreFindsWhatItSaved()
    {
        var store = PulseSurveyRecordStore.NewMemoryPulseSurveyRecordStore();
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
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
        MemoryPulseSurveyRecordStoreFindsWhatItSaved();
    }
}
