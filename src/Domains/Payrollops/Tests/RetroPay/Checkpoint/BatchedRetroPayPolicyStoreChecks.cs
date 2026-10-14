namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedRetroPayPolicyStoreChecks
{
    public static void MemoryBatchedRetroPayPolicyStoreFindsWhatItSaved()
    {
        var store = BatchedRetroPayPolicyStore.NewMemoryBatchedRetroPayPolicyStore();
        var item = BatchedRetroPayPolicy.NewBatchedRetroPayPolicy("code-894", "title-96");
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
        MemoryBatchedRetroPayPolicyStoreFindsWhatItSaved();
    }
}
