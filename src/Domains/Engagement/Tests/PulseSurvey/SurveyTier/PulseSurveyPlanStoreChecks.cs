namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class PulseSurveyPlanStoreChecks
{
    public static void MemoryPulseSurveyPlanStoreFindsWhatItSaved()
    {
        var store = PulseSurveyPlanStore.NewMemoryPulseSurveyPlanStore();
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
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
        MemoryPulseSurveyPlanStoreFindsWhatItSaved();
    }
}
