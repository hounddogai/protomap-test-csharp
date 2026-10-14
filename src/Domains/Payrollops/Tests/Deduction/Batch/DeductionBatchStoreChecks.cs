namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionBatchStoreChecks
{
    public static void MemoryDeductionBatchStoreFindsWhatItSaved()
    {
        var store = DeductionBatchStore.NewMemoryDeductionBatchStore();
        var item = DeductionBatch.NewDeductionBatch("code-654", "label-135");
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
        MemoryDeductionBatchStoreFindsWhatItSaved();
    }
}
