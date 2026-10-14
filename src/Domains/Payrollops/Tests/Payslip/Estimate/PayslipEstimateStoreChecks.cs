namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipEstimateStoreChecks
{
    public static void MemoryPayslipEstimateStoreFindsWhatItSaved()
    {
        var store = PayslipEstimateStore.NewMemoryPayslipEstimateStore();
        var item = PayslipEstimate.NewPayslipEstimate("code-660", "name-552");
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
        MemoryPayslipEstimateStoreFindsWhatItSaved();
    }
}
