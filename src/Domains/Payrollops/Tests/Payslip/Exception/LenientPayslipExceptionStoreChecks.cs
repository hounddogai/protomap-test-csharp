namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientPayslipExceptionStoreChecks
{
    public static void MemoryLenientPayslipExceptionStoreFindsWhatItSaved()
    {
        var store = LenientPayslipExceptionStore.NewMemoryLenientPayslipExceptionStore();
        var item = LenientPayslipException.NewLenientPayslipException("code-136", "label-704");
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
        MemoryLenientPayslipExceptionStoreFindsWhatItSaved();
    }
}
