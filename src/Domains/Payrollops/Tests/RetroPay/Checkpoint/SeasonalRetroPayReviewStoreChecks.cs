namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalRetroPayReviewStoreChecks
{
    public static void MemorySeasonalRetroPayReviewStoreFindsWhatItSaved()
    {
        var store = SeasonalRetroPayReviewStore.NewMemorySeasonalRetroPayReviewStore();
        var item = SeasonalRetroPayReview.NewSeasonalRetroPayReview("code-908", "name-603");
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
        MemorySeasonalRetroPayReviewStoreFindsWhatItSaved();
    }
}
