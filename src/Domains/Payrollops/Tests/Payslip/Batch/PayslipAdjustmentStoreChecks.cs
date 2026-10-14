namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PayslipAdjustmentStoreChecks
{
    public static void MemoryPayslipAdjustmentStoreFindsWhatItSaved()
    {
        var store = PayslipAdjustmentStore.NewMemoryPayslipAdjustmentStore();
        var item = PayslipAdjustment.NewPayslipAdjustment("code-920", "name-767");
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
        MemoryPayslipAdjustmentStoreFindsWhatItSaved();
    }
}
