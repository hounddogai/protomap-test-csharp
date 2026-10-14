namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RateReviewStoreChecks
{
    public static void MemoryRateReviewStoreFindsWhatItSaved()
    {
        var store = RateReviewStore.NewMemoryRateReviewStore();
        var item = RateReview.NewRateReview("code-112", "name-982");
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
        MemoryRateReviewStoreFindsWhatItSaved();
    }
}
