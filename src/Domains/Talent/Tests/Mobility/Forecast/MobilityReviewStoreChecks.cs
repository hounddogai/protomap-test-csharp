namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityReviewStoreChecks
{
    public static void MemoryMobilityReviewStoreFindsWhatItSaved()
    {
        var store = MobilityReviewStore.NewMemoryMobilityReviewStore();
        var item = MobilityReview.NewMobilityReview("code-547", "label-723");
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
        MemoryMobilityReviewStoreFindsWhatItSaved();
    }
}
