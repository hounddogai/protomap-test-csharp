namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunReviewStoreChecks
{
    public static void MemoryPayRunReviewStoreFindsWhatItSaved()
    {
        var store = PayRunReviewStore.NewMemoryPayRunReviewStore();
        var item = PayRunReview.NewPayRunReview("code-995", "title-392");
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
        MemoryPayRunReviewStoreFindsWhatItSaved();
    }
}
