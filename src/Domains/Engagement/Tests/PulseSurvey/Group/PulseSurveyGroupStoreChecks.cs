namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyGroupStoreChecks
{
    public static void MemoryPulseSurveyGroupStoreFindsWhatItSaved()
    {
        var store = PulseSurveyGroupStore.NewMemoryPulseSurveyGroupStore();
        var item = PulseSurveyGroup.NewPulseSurveyGroup("code-13", "label-732");
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
        MemoryPulseSurveyGroupStoreFindsWhatItSaved();
    }
}
