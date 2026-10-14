namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedCareerPathPlanStoreChecks
{
    public static void MemoryBatchedCareerPathPlanStoreFindsWhatItSaved()
    {
        var store = BatchedCareerPathPlanStore.NewMemoryBatchedCareerPathPlanStore();
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("code-596", "name-268");
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
        MemoryBatchedCareerPathPlanStoreFindsWhatItSaved();
    }
}
