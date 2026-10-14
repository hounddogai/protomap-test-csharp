namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class StrictPulseSurveyVarianceStoreChecks
{
    public static void MemoryStrictPulseSurveyVarianceStoreFindsWhatItSaved()
    {
        var store = StrictPulseSurveyVarianceStore.NewMemoryStrictPulseSurveyVarianceStore();
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
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
        MemoryStrictPulseSurveyVarianceStoreFindsWhatItSaved();
    }
}
