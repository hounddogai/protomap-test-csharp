namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalMobilityReviewStoreChecks
{
    public static void MemoryRegionalMobilityReviewStoreFindsWhatItSaved()
    {
        var store = RegionalMobilityReviewStore.NewMemoryRegionalMobilityReviewStore();
        var item = RegionalMobilityReview.NewRegionalMobilityReview("code-726", "title-198");
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
        MemoryRegionalMobilityReviewStoreFindsWhatItSaved();
    }
}
