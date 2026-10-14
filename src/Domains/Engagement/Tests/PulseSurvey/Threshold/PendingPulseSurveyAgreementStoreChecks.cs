namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PendingPulseSurveyAgreementStoreChecks
{
    public static void MemoryPendingPulseSurveyAgreementStoreFindsWhatItSaved()
    {
        var store = PendingPulseSurveyAgreementStore.NewMemoryPendingPulseSurveyAgreementStore();
        var item = PendingPulseSurveyAgreement.NewPendingPulseSurveyAgreement("code-308", "title-461");
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
        MemoryPendingPulseSurveyAgreementStoreFindsWhatItSaved();
    }
}
