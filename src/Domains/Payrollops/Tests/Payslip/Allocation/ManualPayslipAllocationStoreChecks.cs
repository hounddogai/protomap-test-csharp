namespace Acme.Workforce.Payrollops.Tests.Payslip.Allocation;

using Acme.Workforce.Payrollops.Payslip.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualPayslipAllocationStoreChecks
{
    public static void MemoryManualPayslipAllocationStoreFindsWhatItSaved()
    {
        var store = ManualPayslipAllocationStore.NewMemoryManualPayslipAllocationStore();
        var item = ManualPayslipAllocation.NewManualPayslipAllocation("code-756", "name-132");
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
        MemoryManualPayslipAllocationStoreFindsWhatItSaved();
    }
}
