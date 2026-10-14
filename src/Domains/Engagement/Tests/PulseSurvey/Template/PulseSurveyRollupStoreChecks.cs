namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyRollupStoreChecks
{
    public static void MemoryPulseSurveyRollupStoreFindsWhatItSaved()
    {
        var store = PulseSurveyRollupStore.NewMemoryPulseSurveyRollupStore();
        var item = PulseSurveyRollup.NewPulseSurveyRollup("code-84", "title-537");
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
        MemoryPulseSurveyRollupStoreFindsWhatItSaved();
    }
}
