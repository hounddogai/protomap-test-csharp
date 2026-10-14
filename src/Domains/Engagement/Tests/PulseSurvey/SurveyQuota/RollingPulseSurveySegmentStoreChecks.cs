namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class RollingPulseSurveySegmentStoreChecks
{
    public static void MemoryRollingPulseSurveySegmentStoreFindsWhatItSaved()
    {
        var store = RollingPulseSurveySegmentStore.NewMemoryRollingPulseSurveySegmentStore();
        var item = RollingPulseSurveySegment.NewRollingPulseSurveySegment("code-340", "label-650");
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
        MemoryRollingPulseSurveySegmentStoreFindsWhatItSaved();
    }
}
