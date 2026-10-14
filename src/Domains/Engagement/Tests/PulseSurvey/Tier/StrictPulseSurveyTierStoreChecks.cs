namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictPulseSurveyTierStoreChecks
{
    public static void MemoryStrictPulseSurveyTierStoreFindsWhatItSaved()
    {
        var store = StrictPulseSurveyTierStore.NewMemoryStrictPulseSurveyTierStore();
        var item = StrictPulseSurveyTier.NewStrictPulseSurveyTier("code-506", "label-131");
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
        MemoryStrictPulseSurveyTierStoreFindsWhatItSaved();
    }
}
