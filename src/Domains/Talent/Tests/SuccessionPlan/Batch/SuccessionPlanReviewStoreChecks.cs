namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanReviewStoreChecks
{
    public static void MemorySuccessionPlanReviewStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanReviewStore.NewMemorySuccessionPlanReviewStore();
        var item = SuccessionPlanReview.NewSuccessionPlanReview("code-455", "title-185");
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
        MemorySuccessionPlanReviewStoreFindsWhatItSaved();
    }
}
