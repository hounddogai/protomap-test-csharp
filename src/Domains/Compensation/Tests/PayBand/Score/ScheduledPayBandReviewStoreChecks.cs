namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledPayBandReviewStoreChecks
{
    public static void MemoryScheduledPayBandReviewStoreFindsWhatItSaved()
    {
        var store = ScheduledPayBandReviewStore.NewMemoryScheduledPayBandReviewStore();
        var item = ScheduledPayBandReview.NewScheduledPayBandReview("code-914", "title-875");
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
        MemoryScheduledPayBandReviewStoreFindsWhatItSaved();
    }
}
