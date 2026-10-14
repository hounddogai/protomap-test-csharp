namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PulseSurveyMetricStoreChecks
{
    public static void MemoryPulseSurveyMetricStoreFindsWhatItSaved()
    {
        var store = PulseSurveyMetricStore.NewMemoryPulseSurveyMetricStore();
        var item = PulseSurveyMetric.NewPulseSurveyMetric("code-663", "label-649");
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
        MemoryPulseSurveyMetricStoreFindsWhatItSaved();
    }
}
