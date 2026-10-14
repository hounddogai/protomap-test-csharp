namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyReviewStoreChecks
{
    public static void MemoryPulseSurveyReviewStoreFindsWhatItSaved()
    {
        var store = PulseSurveyReviewStore.NewMemoryPulseSurveyReviewStore();
        var item = PulseSurveyReview.NewPulseSurveyReview("code-703", "label-588");
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
        MemoryPulseSurveyReviewStoreFindsWhatItSaved();
    }
}
