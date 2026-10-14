namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class LegacyMeritCycleReviewStoreChecks
{
    public static void MemoryLegacyMeritCycleReviewStoreFindsWhatItSaved()
    {
        var store = LegacyMeritCycleReviewStore.NewMemoryLegacyMeritCycleReviewStore();
        var item = LegacyMeritCycleReview.NewLegacyMeritCycleReview("code-572", "name-40");
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
        MemoryLegacyMeritCycleReviewStoreFindsWhatItSaved();
    }
}
