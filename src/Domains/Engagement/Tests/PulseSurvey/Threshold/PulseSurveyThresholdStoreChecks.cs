namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyThresholdStoreChecks
{
    public static void MemoryPulseSurveyThresholdStoreFindsWhatItSaved()
    {
        var store = PulseSurveyThresholdStore.NewMemoryPulseSurveyThresholdStore();
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("code-926", "label-997");
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
        MemoryPulseSurveyThresholdStoreFindsWhatItSaved();
    }
}
