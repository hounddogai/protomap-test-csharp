namespace Acme.Workforce.Payrollops.Tests.Payslip.Claim;

using Acme.Workforce.Payrollops.Payslip.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PayslipBaselineStoreChecks
{
    public static void MemoryPayslipBaselineStoreFindsWhatItSaved()
    {
        var store = PayslipBaselineStore.NewMemoryPayslipBaselineStore();
        var item = PayslipBaseline.NewPayslipBaseline("code-7", "title-931");
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
        MemoryPayslipBaselineStoreFindsWhatItSaved();
    }
}
