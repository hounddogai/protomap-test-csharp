namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyProfileStoreChecks
{
    public static void MemoryCappedPulseSurveyProfileStoreFindsWhatItSaved()
    {
        var store = CappedPulseSurveyProfileStore.NewMemoryCappedPulseSurveyProfileStore();
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
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
        MemoryCappedPulseSurveyProfileStoreFindsWhatItSaved();
    }
}
