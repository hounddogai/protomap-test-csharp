namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandReviewStoreChecks
{
    public static void MemoryPayBandReviewStoreFindsWhatItSaved()
    {
        var store = PayBandReviewStore.NewMemoryPayBandReviewStore();
        var item = PayBandReview.NewPayBandReview("code-647", "name-275");
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
        MemoryPayBandReviewStoreFindsWhatItSaved();
    }
}
