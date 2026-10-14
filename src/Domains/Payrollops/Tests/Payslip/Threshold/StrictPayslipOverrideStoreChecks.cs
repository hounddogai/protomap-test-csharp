namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideStoreChecks
{
    public static void MemoryStrictPayslipOverrideStoreFindsWhatItSaved()
    {
        var store = StrictPayslipOverrideStore.NewMemoryStrictPayslipOverrideStore();
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
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
        MemoryStrictPayslipOverrideStoreFindsWhatItSaved();
    }
}
