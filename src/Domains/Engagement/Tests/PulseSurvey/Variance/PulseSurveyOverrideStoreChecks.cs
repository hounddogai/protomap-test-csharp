namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyOverrideStoreChecks
{
    public static void MemoryPulseSurveyOverrideStoreFindsWhatItSaved()
    {
        var store = PulseSurveyOverrideStore.NewMemoryPulseSurveyOverrideStore();
        var item = PulseSurveyOverride.NewPulseSurveyOverride("code-232", "name-981");
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
        MemoryPulseSurveyOverrideStoreFindsWhatItSaved();
    }
}
