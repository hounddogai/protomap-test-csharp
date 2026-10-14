namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackPulseSurveySegmentStoreChecks
{
    public static void MemoryFallbackPulseSurveySegmentStoreFindsWhatItSaved()
    {
        var store = FallbackPulseSurveySegmentStore.NewMemoryFallbackPulseSurveySegmentStore();
        var item = FallbackPulseSurveySegment.NewFallbackPulseSurveySegment("code-226", "name-42");
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
        MemoryFallbackPulseSurveySegmentStoreFindsWhatItSaved();
    }
}
