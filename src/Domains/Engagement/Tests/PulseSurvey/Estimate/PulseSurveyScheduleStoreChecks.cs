namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyScheduleStoreChecks
{
    public static void MemoryPulseSurveyScheduleStoreFindsWhatItSaved()
    {
        var store = PulseSurveyScheduleStore.NewMemoryPulseSurveyScheduleStore();
        var item = PulseSurveySchedule.NewPulseSurveySchedule("code-440", "title-558");
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
        MemoryPulseSurveyScheduleStoreFindsWhatItSaved();
    }
}
