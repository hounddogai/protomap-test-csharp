namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CappedPayslipBatchStoreChecks
{
    public static void MemoryCappedPayslipBatchStoreFindsWhatItSaved()
    {
        var store = CappedPayslipBatchStore.NewMemoryCappedPayslipBatchStore();
        var item = CappedPayslipBatch.NewCappedPayslipBatch("code-23", "label-81");
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
        MemoryCappedPayslipBatchStoreFindsWhatItSaved();
    }
}
