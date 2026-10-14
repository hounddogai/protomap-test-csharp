namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveFeedbackForecastStoreChecks
{
    public static void MemoryAdaptiveFeedbackForecastStoreFindsWhatItSaved()
    {
        var store = AdaptiveFeedbackForecastStore.NewMemoryAdaptiveFeedbackForecastStore();
        var item = AdaptiveFeedbackForecast.NewAdaptiveFeedbackForecast("code-458", "title-390");
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
        MemoryAdaptiveFeedbackForecastStoreFindsWhatItSaved();
    }
}
