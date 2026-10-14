namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Claim;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingPulseSurveyClaimStoreChecks
{
    public static void MemoryPendingPulseSurveyClaimStoreFindsWhatItSaved()
    {
        var store = PendingPulseSurveyClaimStore.NewMemoryPendingPulseSurveyClaimStore();
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
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
        MemoryPendingPulseSurveyClaimStoreFindsWhatItSaved();
    }
}
