namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class GlobalRateReviewStoreChecks
{
    public static void MemoryGlobalRateReviewStoreFindsWhatItSaved()
    {
        var store = GlobalRateReviewStore.NewMemoryGlobalRateReviewStore();
        var item = GlobalRateReview.NewGlobalRateReview("code-216", "label-528");
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
        MemoryGlobalRateReviewStoreFindsWhatItSaved();
    }
}
