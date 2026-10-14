namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveySummaryStoreChecks
{
    public static void MemoryPulseSurveySummaryStoreFindsWhatItSaved()
    {
        var store = PulseSurveySummaryStore.NewMemoryPulseSurveySummaryStore();
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
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
        MemoryPulseSurveySummaryStoreFindsWhatItSaved();
    }
}
