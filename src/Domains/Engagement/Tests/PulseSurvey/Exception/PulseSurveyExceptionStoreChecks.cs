namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyExceptionStoreChecks
{
    public static void MemoryPulseSurveyExceptionStoreFindsWhatItSaved()
    {
        var store = PulseSurveyExceptionStore.NewMemoryPulseSurveyExceptionStore();
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
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
        MemoryPulseSurveyExceptionStoreFindsWhatItSaved();
    }
}
