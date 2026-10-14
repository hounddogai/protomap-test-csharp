namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyRequestStoreChecks
{
    public static void MemoryPulseSurveyRequestStoreFindsWhatItSaved()
    {
        var store = PulseSurveyRequestStore.NewMemoryPulseSurveyRequestStore();
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
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
        MemoryPulseSurveyRequestStoreFindsWhatItSaved();
    }
}
