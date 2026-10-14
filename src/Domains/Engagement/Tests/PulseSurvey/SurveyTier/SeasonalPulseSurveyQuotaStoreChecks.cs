namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaStoreChecks
{
    public static void MemorySeasonalPulseSurveyQuotaStoreFindsWhatItSaved()
    {
        var store = SeasonalPulseSurveyQuotaStore.NewMemorySeasonalPulseSurveyQuotaStore();
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
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
        MemorySeasonalPulseSurveyQuotaStoreFindsWhatItSaved();
    }
}
