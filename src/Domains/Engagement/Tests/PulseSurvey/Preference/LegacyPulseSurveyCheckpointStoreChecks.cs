namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LegacyPulseSurveyCheckpointStoreChecks
{
    public static void MemoryLegacyPulseSurveyCheckpointStoreFindsWhatItSaved()
    {
        var store = LegacyPulseSurveyCheckpointStore.NewMemoryLegacyPulseSurveyCheckpointStore();
        var item = LegacyPulseSurveyCheckpoint.NewLegacyPulseSurveyCheckpoint("code-583", "label-71");
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
        MemoryLegacyPulseSurveyCheckpointStoreFindsWhatItSaved();
    }
}
