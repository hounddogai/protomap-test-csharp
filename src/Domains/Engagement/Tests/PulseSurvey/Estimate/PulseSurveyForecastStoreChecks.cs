namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastStoreChecks
{
    public static void MemoryPulseSurveyForecastStoreFindsWhatItSaved()
    {
        var store = PulseSurveyForecastStore.NewMemoryPulseSurveyForecastStore();
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
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
        MemoryPulseSurveyForecastStoreFindsWhatItSaved();
    }
}
