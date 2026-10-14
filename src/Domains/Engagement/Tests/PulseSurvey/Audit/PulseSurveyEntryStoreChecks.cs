namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyEntryStoreChecks
{
    public static void MemoryPulseSurveyEntryStoreFindsWhatItSaved()
    {
        var store = PulseSurveyEntryStore.NewMemoryPulseSurveyEntryStore();
        var item = PulseSurveyEntry.NewPulseSurveyEntry("code-242", "title-420");
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
        MemoryPulseSurveyEntryStoreFindsWhatItSaved();
    }
}
