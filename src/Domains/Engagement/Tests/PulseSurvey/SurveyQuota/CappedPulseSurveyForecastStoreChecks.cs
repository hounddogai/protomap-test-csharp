namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyForecastStoreChecks
{
    public static void MemoryCappedPulseSurveyForecastStoreFindsWhatItSaved()
    {
        var store = CappedPulseSurveyForecastStore.NewMemoryCappedPulseSurveyForecastStore();
        var item = CappedPulseSurveyForecast.NewCappedPulseSurveyForecast("code-350", "title-456");
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
        MemoryCappedPulseSurveyForecastStoreFindsWhatItSaved();
    }
}
