namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyDraftStoreChecks
{
    public static void MemoryPulseSurveyDraftStoreFindsWhatItSaved()
    {
        var store = PulseSurveyDraftStore.NewMemoryPulseSurveyDraftStore();
        var item = PulseSurveyDraft.NewPulseSurveyDraft("code-340", "name-453");
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
        MemoryPulseSurveyDraftStoreFindsWhatItSaved();
    }
}
