namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class ManualPulseSurveyTierStoreChecks
{
    public static void MemoryManualPulseSurveyTierStoreFindsWhatItSaved()
    {
        var store = ManualPulseSurveyTierStore.NewMemoryManualPulseSurveyTierStore();
        var item = ManualPulseSurveyTier.NewManualPulseSurveyTier("code-445", "label-314");
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
        MemoryManualPulseSurveyTierStoreFindsWhatItSaved();
    }
}
