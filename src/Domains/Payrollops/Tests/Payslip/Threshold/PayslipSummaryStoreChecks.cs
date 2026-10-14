namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipSummaryStoreChecks
{
    public static void MemoryPayslipSummaryStoreFindsWhatItSaved()
    {
        var store = PayslipSummaryStore.NewMemoryPayslipSummaryStore();
        var item = PayslipSummary.NewPayslipSummary("code-585", "name-840");
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
        MemoryPayslipSummaryStoreFindsWhatItSaved();
    }
}
