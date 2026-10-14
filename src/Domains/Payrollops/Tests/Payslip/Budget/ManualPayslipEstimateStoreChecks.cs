namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ManualPayslipEstimateStoreChecks
{
    public static void MemoryManualPayslipEstimateStoreFindsWhatItSaved()
    {
        var store = ManualPayslipEstimateStore.NewMemoryManualPayslipEstimateStore();
        var item = ManualPayslipEstimate.NewManualPayslipEstimate("code-223", "title-689");
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
        MemoryManualPayslipEstimateStoreFindsWhatItSaved();
    }
}
