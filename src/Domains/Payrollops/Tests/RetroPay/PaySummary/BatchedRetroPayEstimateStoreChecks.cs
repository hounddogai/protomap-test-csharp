namespace Acme.Workforce.Payrollops.Tests.RetroPay.PaySummary;

using Acme.Workforce.Payrollops.RetroPay.PaySummary;

/// <summary>Checks of pay summary code, run by the project's runner.</summary>
public static class BatchedRetroPayEstimateStoreChecks
{
    public static void MemoryBatchedRetroPayEstimateStoreFindsWhatItSaved()
    {
        var store = BatchedRetroPayEstimateStore.NewMemoryBatchedRetroPayEstimateStore();
        var item = BatchedRetroPayEstimate.NewBatchedRetroPayEstimate("code-668", "title-90");
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
        MemoryBatchedRetroPayEstimateStoreFindsWhatItSaved();
    }
}
