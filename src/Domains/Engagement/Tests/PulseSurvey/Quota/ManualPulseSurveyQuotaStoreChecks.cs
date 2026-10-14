namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyQuotaStoreChecks
{
    public static void MemoryManualPulseSurveyQuotaStoreFindsWhatItSaved()
    {
        var store = ManualPulseSurveyQuotaStore.NewMemoryManualPulseSurveyQuotaStore();
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("code-670", "name-524");
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
        MemoryManualPulseSurveyQuotaStoreFindsWhatItSaved();
    }
}
