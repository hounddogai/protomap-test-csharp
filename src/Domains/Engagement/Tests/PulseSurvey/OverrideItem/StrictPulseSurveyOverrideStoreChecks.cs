namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPulseSurveyOverrideStoreChecks
{
    public static void MemoryStrictPulseSurveyOverrideStoreFindsWhatItSaved()
    {
        var store = StrictPulseSurveyOverrideStore.NewMemoryStrictPulseSurveyOverrideStore();
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
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
        MemoryStrictPulseSurveyOverrideStoreFindsWhatItSaved();
    }
}
