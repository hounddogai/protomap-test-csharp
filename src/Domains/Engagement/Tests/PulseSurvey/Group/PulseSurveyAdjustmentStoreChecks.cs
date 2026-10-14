namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Group;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PulseSurveyAdjustmentStoreChecks
{
    public static void MemoryPulseSurveyAdjustmentStoreFindsWhatItSaved()
    {
        var store = PulseSurveyAdjustmentStore.NewMemoryPulseSurveyAdjustmentStore();
        var item = PulseSurveyAdjustment.NewPulseSurveyAdjustment("code-74", "name-632");
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
        MemoryPulseSurveyAdjustmentStoreFindsWhatItSaved();
    }
}
