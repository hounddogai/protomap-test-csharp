namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayPolicy;

using Acme.Workforce.Payrollops.RetroPay.PayPolicy;

/// <summary>Checks of pay policy code, run by the project's runner.</summary>
public static class BatchedRetroPayPlanStoreChecks
{
    public static void MemoryBatchedRetroPayPlanStoreFindsWhatItSaved()
    {
        var store = BatchedRetroPayPlanStore.NewMemoryBatchedRetroPayPlanStore();
        var item = BatchedRetroPayPlan.NewBatchedRetroPayPlan("code-443", "name-50");
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
        MemoryBatchedRetroPayPlanStoreFindsWhatItSaved();
    }
}
