namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class LenientPayslipRollupStoreChecks
{
    public static void MemoryLenientPayslipRollupStoreFindsWhatItSaved()
    {
        var store = LenientPayslipRollupStore.NewMemoryLenientPayslipRollupStore();
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
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
        MemoryLenientPayslipRollupStoreFindsWhatItSaved();
    }
}
