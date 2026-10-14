namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackEstimateStoreChecks
{
    public static void MemorySeasonalFeedbackEstimateStoreFindsWhatItSaved()
    {
        var store = SeasonalFeedbackEstimateStore.NewMemorySeasonalFeedbackEstimateStore();
        var item = SeasonalFeedbackEstimate.NewSeasonalFeedbackEstimate("code-815", "label-859");
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
        MemorySeasonalFeedbackEstimateStoreFindsWhatItSaved();
    }
}
