namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientPulseSurveyManifestStoreChecks
{
    public static void MemoryLenientPulseSurveyManifestStoreFindsWhatItSaved()
    {
        var store = LenientPulseSurveyManifestStore.NewMemoryLenientPulseSurveyManifestStore();
        var item = LenientPulseSurveyManifest.NewLenientPulseSurveyManifest("code-604", "label-301");
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
        MemoryLenientPulseSurveyManifestStoreFindsWhatItSaved();
    }
}
