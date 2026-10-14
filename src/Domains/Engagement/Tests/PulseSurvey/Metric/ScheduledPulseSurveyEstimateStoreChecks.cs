namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyEstimateStoreChecks
{
    public static void MemoryScheduledPulseSurveyEstimateStoreFindsWhatItSaved()
    {
        var store = ScheduledPulseSurveyEstimateStore.NewMemoryScheduledPulseSurveyEstimateStore();
        var item = ScheduledPulseSurveyEstimate.NewScheduledPulseSurveyEstimate("code-170", "title-555");
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
        MemoryScheduledPulseSurveyEstimateStoreFindsWhatItSaved();
    }
}
