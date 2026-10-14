namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyMappingStoreChecks
{
    public static void MemoryPulseSurveyMappingStoreFindsWhatItSaved()
    {
        var store = PulseSurveyMappingStore.NewMemoryPulseSurveyMappingStore();
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
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
        MemoryPulseSurveyMappingStoreFindsWhatItSaved();
    }
}
