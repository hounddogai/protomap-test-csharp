namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Checks of survey audit code, run by the project's runner.</summary>
public static class GlobalPulseSurveyAdjustmentStoreChecks
{
    public static void MemoryGlobalPulseSurveyAdjustmentStoreFindsWhatItSaved()
    {
        var store = GlobalPulseSurveyAdjustmentStore.NewMemoryGlobalPulseSurveyAdjustmentStore();
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("code-881", "label-289");
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
        MemoryGlobalPulseSurveyAdjustmentStoreFindsWhatItSaved();
    }
}
