namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyAdjustmentStoreChecks
{
    public static void MemoryScheduledPulseSurveyAdjustmentStoreFindsWhatItSaved()
    {
        var store = ScheduledPulseSurveyAdjustmentStore.NewMemoryScheduledPulseSurveyAdjustmentStore();
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
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
        MemoryScheduledPulseSurveyAdjustmentStoreFindsWhatItSaved();
    }
}
