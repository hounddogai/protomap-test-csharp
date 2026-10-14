namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestStoreChecks
{
    public static void MemoryManualPulseSurveyDigestStoreFindsWhatItSaved()
    {
        var store = ManualPulseSurveyDigestStore.NewMemoryManualPulseSurveyDigestStore();
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
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
        MemoryManualPulseSurveyDigestStoreFindsWhatItSaved();
    }
}
