namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyAuditStoreChecks
{
    public static void MemorySeasonalPulseSurveyAuditStoreFindsWhatItSaved()
    {
        var store = SeasonalPulseSurveyAuditStore.NewMemorySeasonalPulseSurveyAuditStore();
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
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
        MemorySeasonalPulseSurveyAuditStoreFindsWhatItSaved();
    }
}
