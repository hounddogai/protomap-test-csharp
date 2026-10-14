namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateStoreChecks
{
    public static void MemoryPulseSurveyEstimateStoreFindsWhatItSaved()
    {
        var store = PulseSurveyEstimateStore.NewMemoryPulseSurveyEstimateStore();
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
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
        MemoryPulseSurveyEstimateStoreFindsWhatItSaved();
    }
}
