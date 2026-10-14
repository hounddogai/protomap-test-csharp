namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotStoreChecks
{
    public static void MemoryPulseSurveySlotStoreFindsWhatItSaved()
    {
        var store = PulseSurveySlotStore.NewMemoryPulseSurveySlotStore();
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
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
        MemoryPulseSurveySlotStoreFindsWhatItSaved();
    }
}
