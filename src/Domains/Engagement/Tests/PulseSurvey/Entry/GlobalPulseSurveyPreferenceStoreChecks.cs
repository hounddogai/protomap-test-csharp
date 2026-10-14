namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalPulseSurveyPreferenceStoreChecks
{
    public static void MemoryGlobalPulseSurveyPreferenceStoreFindsWhatItSaved()
    {
        var store = GlobalPulseSurveyPreferenceStore.NewMemoryGlobalPulseSurveyPreferenceStore();
        var item = GlobalPulseSurveyPreference.NewGlobalPulseSurveyPreference("code-778", "title-778");
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
        MemoryGlobalPulseSurveyPreferenceStoreFindsWhatItSaved();
    }
}
